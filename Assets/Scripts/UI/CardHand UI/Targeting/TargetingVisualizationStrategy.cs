using UnityEngine;

public class TargetingVisualizationStrategy : MonoBehaviour{
    [SerializeField] private Transform playerPortrait;
    [SerializeField] private BoardInputManager boardInputManager;

    // Префаби для створення візуалізації
    [SerializeField] private CardMovementTargeting cardTargeting;
    [SerializeField] private ArrowTargeting arrowTargeting;

    public TargetingVisualizationStrategy(Transform playerPortrait, BoardInputManager boardInputManager) {
        this.playerPortrait = playerPortrait;
        this.boardInputManager = boardInputManager;
    }

    public ITargetingVisualization CreateVisualization(TargetSelectionRequest request) {
        if (request.Initiator is CardPresenter card) {
            return IsZoneRequirement(request.Requirement)
                ? CreateCardMovementTargeting(card)
                : CreateArrowTargeting(card.transform.position, request);
        }

        return CreateArrowTargeting(playerPortrait.position, request);
    }


    private bool IsZoneRequirement(ITargetRequirement requirement) {
        return requirement is ZoneRequirement;
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
