using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using Zenject;

public class BoardSeatSystem : MonoBehaviour {
    public Action OnSeatsTook;
    [Header("Seats")]
    [SerializeField] private BoardSeat playerSeat;
    [SerializeField] private BoardSeat enemySeat;

    private Dictionary<Opponent, BoardSeat> activeSeatsByOpponent = new();
    [Inject] OpponentRegistrator opponentRegistrator;
    private void Awake() {
        if (opponentRegistrator == null) return;

        if (opponentRegistrator.IsMatchReady) {
            PrepareBattle(
                opponentRegistrator.PlayerPresenter,
                opponentRegistrator.EnemyPresenter
                );
        } else {
            opponentRegistrator.OnMatchSetup += PrepareBattle;
        }
    }

    private void PrepareBattle(PlayerPresenter player, EnemyPresenter enemy) {
        TookSeats(player, enemy).Forget();
    }

    public async UniTask TookSeats(PlayerPresenter player, EnemyPresenter enemy) {
        if (player == null) {
            Debug.LogError("Player is null");
        }
        if (enemy == null) {
            Debug.LogError("Enemy is null");
        }
        await UniTask.WhenAll(
            AssignOpponentSeat(enemy),
            AssignOpponentSeat(player)
        );

        OnSeatsTook?.Invoke();
    }

    public async UniTask AssignOpponentSeat(OpponentPresenter presenter) {
        Opponent model = presenter.Model;
        BoardSeat seat = model is Player ? playerSeat : enemySeat;
        
        seat.AssignOpponent(model);
        activeSeatsByOpponent[model] = seat;
        await presenter.MoveToSeat(seat);
    }

    public void InitializePlayersCardsSystems() {
        enemySeat.InitCards();
        playerSeat.InitCards();
    }

    public void ClearAllSeats() {
        foreach (var seat in activeSeatsByOpponent.Values) {
            seat.ClearSeat();
        }

        activeSeatsByOpponent.Clear();
    }

    public ITargetingService GetActionFiller(Opponent opponent) {
        if (activeSeatsByOpponent.TryGetValue(opponent, out BoardSeat seat)) {
            throw new NotImplementedException();
        }
        return null;
    }

    public Opponent GetAgainstOpponent(Opponent opponent) {
        return opponent is Player ? enemySeat.GetOwner() : playerSeat.GetOwner();
    }

    public bool GetOpponentDirection(Opponent opponent, out Direction direction) {
        if (activeSeatsByOpponent.TryGetValue(opponent, out BoardSeat seat)) {
            direction = seat.Direction;
            return true;
        }

        direction = default;
        return false;
    }

    public BoardSeat GetSeatByOpponent(Opponent opponent) {
        if (activeSeatsByOpponent.TryGetValue(opponent, out BoardSeat seat)) {
            return seat;
        }
        return null;
    }

    public IEnumerable<Opponent> GetAllOpponents() {
        return activeSeatsByOpponent.Keys;
    }

    public bool IsReady() {
        return activeSeatsByOpponent.Count == 2;
    }

    public Dictionary<Opponent, Direction> GetAllOpponentDirections() {
        return activeSeatsByOpponent.ToDictionary(
            pair => pair.Key,
            pair => pair.Value.Direction
        );
    }
}

