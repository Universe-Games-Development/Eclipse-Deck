using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

public class BoardSeat : MonoBehaviour {
    public Direction Direction;
    //[SerializeField] private Transform SeatTransform;
    [SerializeField] private HealthCellView HealthCell;
    [SerializeField] private CardsHandleSystem cardsPlaySystem;
    public Opponent CurrentOpponent { get; private set; }
   


    public async UniTask AssignOpponent(Opponent opponent) {

        if (opponent == null) {
            return;
        }
        CurrentOpponent = opponent;

        await CurrentOpponent.TakeSeat(this);

        // Initialize health display if available
        if (HealthCell != null) {
            HealthCell.Initialize();
            HealthCell.AssignOwner(GetOwner());
        }
    }

    public void ClearSeat() {
        if (CurrentOpponent != null) {
            CurrentOpponent.ClearSeat().Forget();
            CurrentOpponent = null;
        }

        if (HealthCell != null) {
            HealthCell.ClearOwner();
        }
    }

    public void InitCards() {
        cardsPlaySystem.Initialize(GetOwner());
    }

    public Opponent GetOwner() {
        return CurrentOpponent;
    }

    private void OnDrawGizmosSelected() {
        Gizmos.DrawSphere(transform.position, 1f);
    }

    
}

