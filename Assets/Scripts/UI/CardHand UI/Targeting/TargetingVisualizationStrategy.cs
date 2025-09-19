using UnityEngine;
using Zenject;

public class TargetingVisualizationStrategy : MonoBehaviour {
    [SerializeField] private Transform playerPortrait;
    [SerializeField] private BoardInputManager boardInputManager;

    // Префаби для створення візуалізації
    [SerializeField] private CardMovementTargeting cardTargeting;
    [SerializeField] private ArrowTargeting arrowTargeting;
    [Inject] IUnitPresenterRegistry presenterRegistry;

    public TargetingVisualizationStrategy(Transform playerPortrait, BoardInputManager boardInputManager) {
        this.playerPortrait = playerPortrait;
        this.boardInputManager = boardInputManager;
    }

    public ITargetingVisualization CreateVisualization(TargetSelectionRequest request) {
        if (request.Source is Card card) {
            CardPresenter cardPresenter = presenterRegistry.GetPresenter<CardPresenter>(card);
            if (IsZoneRequirement(request.Target)) {
                return CreateCardMovementTargeting(cardPresenter);
            } else {
                return CreateArrowTargeting(cardPresenter.transform.position, request);
            }

        }

        return CreateArrowTargeting(playerPortrait.position, request);
    }

    private bool IsZoneRequirement(TypedTargetBase request) {
        return request.TargetType == typeof(Zone);
    }

    private ITargetingVisualization CreateCardMovementTargeting(CardPresenter card) {
        cardTargeting.Initialize(card);
        return cardTargeting;
    }

    private ITargetingVisualization CreateArrowTargeting(Vector3 startPos, TargetSelectionRequest request) {
        arrowTargeting.Initialize(startPos, request);
        return arrowTargeting;
    }
}
