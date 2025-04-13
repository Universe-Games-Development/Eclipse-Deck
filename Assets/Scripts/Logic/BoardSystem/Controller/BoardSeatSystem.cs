using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class BoardSeatSystem : MonoBehaviour {
    [Header ("Seats")]
    [SerializeField] private BoardSeat playerSeat;
    [SerializeField] private BoardSeat enemySeat;

    private Dictionary<Opponent, BoardSeat> activeSeatsByOpponent = new();

    public async UniTask AssignOpponentSeat(BaseOpponentPresenter presenter) {
        Opponent opponent = presenter.OpponentModel;
        BoardSeat seat = opponent is Player ? playerSeat : enemySeat;

        await seat.AssignOpponent(opponent, presenter);
        activeSeatsByOpponent[opponent] = seat;
    }

    public void InitializePlayersCards() {
        enemySeat.InitCards();
        playerSeat.InitCards(); 
    }

    public void ClearAllSeats() {
        foreach (var seat in activeSeatsByOpponent.Values) {
            seat.ClearSeat();
        }

        activeSeatsByOpponent.Clear();
    }

    public IActionFiller GetActionFiller(Opponent opponent) {
        if (activeSeatsByOpponent.TryGetValue(opponent, out BoardSeat seat)) {
            return seat.CurrentPresenter.GetActionFiller();
        }
        return null;
    }

    public Opponent GetAgainstOpponent(Opponent opponent) {
        return opponent is Player ? enemySeat.Owner : playerSeat.Owner;
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

    public Dictionary<Opponent, Direction> GetAllOpponentDirections() {
        return activeSeatsByOpponent.ToDictionary(
            pair => pair.Key,
            pair => pair.Value.Direction
        );
    }
}

