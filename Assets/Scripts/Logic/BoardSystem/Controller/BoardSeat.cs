using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using Zenject;

public class BoardSeat : MonoBehaviour, IGameUnit {
    [Inject] TurnManager _turnManager; 
    public Direction Direction;
    //[SerializeField] private Transform SeatTransform;
    [SerializeField] private HealthCellView HealthCell;
    [SerializeField] private CardsHandleSystem cardsPlaySystem;

    public event Action<GameEnterEvent> OnUnitDeployed;

    public Opponent ControlOpponent { get; private set; }

    public EffectManager EffectManager { get; private set; }

    public async UniTask AssignOpponent(Opponent opponent) {
        if (opponent == null) {
            return;
        }
        ControlOpponent = opponent;

        await ControlOpponent.TakeSeat(this);

        // Initialize health display if available
        if (HealthCell != null) {
            HealthCell.Initialize();
            HealthCell.AssignOwner(GetOwner());
        }

        EffectManager = new EffectManager(_turnManager);
        OnUnitDeployed?.Invoke(new GameEnterEvent(this));
    }

    public void ClearSeat() {
        if (ControlOpponent != null) {
            ControlOpponent.ClearSeat().Forget();
            ControlOpponent = null;
        }

        if (HealthCell != null) {
            HealthCell.ClearOwner();
        }
    }

    public void InitCards() {
        cardsPlaySystem.Initialize(GetOwner());
    }

    public Opponent GetOwner() {
        return ControlOpponent;
    }

    private void OnDrawGizmosSelected() {
        Gizmos.DrawSphere(transform.position, 1f);
    }

    
}

