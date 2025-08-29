using UnityEngine;

public class TargetingVisualizationFactory : MonoBehaviour {
    [SerializeField] private ArrowTargeting arrowTargeting;
    [SerializeField] private CardMovementController cardMovementVisualization;

    public ITargetingVisualization CreateVisualization(TargetSelectionRequest request, CardPresenter currentCard) {
        if (request.Requirement is ZoneRequirement && currentCard != null) {
            return cardMovementVisualization;
        } else {
            return arrowTargeting;
        }
    }
}