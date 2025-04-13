using UnityEngine;
using Cysharp.Threading.Tasks;

public class BoardSeat : MonoBehaviour {
    public Direction Direction;
    [SerializeField] private Transform SeatTransform;
    [SerializeField] private HealthCellView HealthCell;
    [SerializeField] private CardsHandleSystem cardsPlaySystem;
    public BaseOpponentPresenter CurrentPresenter { get; private set; }
    public Opponent Owner { get; private set; }
    // Saved data
    private Vector3 presenterOriginalPosition;
    private Transform presenterOriginalParent;


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
                HealthCell.AssignOwner(Owner);
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

    public void InitCards() {
        cardsPlaySystem.Initialize(CurrentPresenter);
    }
}

