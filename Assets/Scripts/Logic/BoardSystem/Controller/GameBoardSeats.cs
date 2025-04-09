using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

public class BoardSeatSystem : MonoBehaviour {
    [SerializeField] private BoardSeatDefinition playerSeatDefinition;
    [SerializeField] private BoardSeatDefinition enemySeatDefinition;

    private Dictionary<Opponent, BoardSeat> activeSeatsByOpponent = new();

    // Inner class that handles a single seat
    public class BoardSeat {
        public Direction Direction { get; private set; }
        public Transform SeatTransform { get; private set; }
        public HealthCellView HealthCell { get; private set; }
        public BaseOpponentPresenter CurrentPresenter { get; private set; }
        public Opponent Owner { get; private set; }

        private Vector3 presenterOriginalPosition;
        private Transform presenterOriginalParent;

        public BoardSeat(Direction direction, Transform seatTransform, HealthCellView healthCell) {
            Direction = direction;
            SeatTransform = seatTransform;
            HealthCell = healthCell;
        }

        public async UniTask AssignOpponent(Opponent opponent, BaseOpponentPresenter presenter) {
            Owner = opponent;

            if (presenter != null) {
                presenterOriginalPosition = presenter.transform.position;
                presenterOriginalParent = presenter.transform.parent;
                CurrentPresenter = presenter;

                await presenter.MoveTo(SeatTransform);
                
                presenter.transform.SetParent(SeatTransform);

                // Initialize health display if available
                if (HealthCell != null) {
                    HealthCell.Initialize();
                    HealthCell.AssignOwner(opponent);
                }
            }
        }

        public void ClearSeat() {
            if (CurrentPresenter != null) {
                CurrentPresenter.transform.SetParent(presenterOriginalParent);
                CurrentPresenter.transform.position = presenterOriginalPosition;
                CurrentPresenter = null;
            }

            if (HealthCell != null) {
                HealthCell.ClearOwner();
            }

            Owner = null;
        }
    }

    private void Awake() {
        // Validate required components
        if (playerSeatDefinition.seatTransform == null || enemySeatDefinition.seatTransform == null) {
            Debug.LogError("Seat transforms not properly assigned!");
        }
    }

    // Initialize a player seat
    public async UniTask AssignPlayerSeat(Player player, BaseOpponentPresenter playerPresenter) {
        BoardSeat playerSeat = new(
            playerSeatDefinition.direction,
            playerSeatDefinition.seatTransform,
            playerSeatDefinition.healthCell
        );

        await playerSeat.AssignOpponent(player, playerPresenter);
        activeSeatsByOpponent[player] = playerSeat;
    }

    // Initialize an enemy seat
    public async UniTask AssignEnemySeat(Opponent enemy, BaseOpponentPresenter enemyPresenter) {
        BoardSeat enemySeat = new(
            enemySeatDefinition.direction,
            enemySeatDefinition.seatTransform,
            enemySeatDefinition.healthCell
        );

        await enemySeat.AssignOpponent(enemy, enemyPresenter);
        activeSeatsByOpponent[enemy] = enemySeat;
    }

    // Get direction for an opponent
    public bool GetOpponentDirection(Opponent opponent, out Direction direction) {
        if (activeSeatsByOpponent.TryGetValue(opponent, out BoardSeat seat)) {
            direction = seat.Direction;
            return true;
        }

        direction = default;
        return false;
    }

    // Get all opponent directions
    public Dictionary<Opponent, Direction> GetAllOpponentDirections() {
        return activeSeatsByOpponent.ToDictionary(
            pair => pair.Key,
            pair => pair.Value.Direction
        );
    }

    // Clear all seats
    public void ClearAllSeats() {
        foreach (var seat in activeSeatsByOpponent.Values) {
            seat.ClearSeat();
        }

        activeSeatsByOpponent.Clear();
    }

    // Get seat by opponent
    public BoardSeat GetSeatByOpponent(Opponent opponent) {
        if (activeSeatsByOpponent.TryGetValue(opponent, out BoardSeat seat)) {
            return seat;
        }
        return null;
    }

    public IEnumerable<Opponent> GetAllOpponents() {
        return activeSeatsByOpponent.Keys;
    }
}

[System.Serializable]
public class BoardSeatDefinition {
    public Direction direction;
    public Transform seatTransform;
    public HealthCellView healthCell;
}
