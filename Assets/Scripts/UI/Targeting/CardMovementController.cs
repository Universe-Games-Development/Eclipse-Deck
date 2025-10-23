using UnityEngine;

public class CardMovementTargeting : MonoBehaviour, ITargetingVisualization {
    [Header("Movement Settings")]
    [SerializeField] private Vector3 cardOffset = new Vector3(0f, 1.2f, 0f);
    [SerializeField] private bool useCameraRayPositioning = true;
    [SerializeField] private int playRenderOrderBoost = 50;

    private CardPresenter cardPresenter;
    private CardView cardView;

    public void Initialize(CardPresenter presenter) {
        cardPresenter = presenter;
        cardView = cardPresenter.CardView;
    }

    public void StartTargeting() {
        if (cardView != null) {
            cardView.ModifyRenderOrder(playRenderOrderBoost);
            cardView.ToggleTiling(true);
        }
    }

    public void UpdateTargeting(Vector3 cursorPosition) {
        if (cardView != null) {
            Vector3 targetPosition = CalculateCardPosition(cursorPosition);
            cardView.DoPhysicsMovement(targetPosition);
        }
    }

    public void StopTargeting() {
        if (cardView != null) {
            cardView.ModifyRenderOrder(-playRenderOrderBoost);
            cardView.ToggleTiling(false);
            cardView.StopMovement();
        }
        cardPresenter = null;
        cardView = null;
    }

    private Vector3 CalculateCardPosition(Vector3 boardPosition) {
        if (!useCameraRayPositioning)
            return boardPosition + cardOffset;

        Camera mainCamera = Camera.main;
        if (mainCamera == null)
            return boardPosition + cardOffset;

        Vector3 cameraPosition = mainCamera.transform.position;
        Vector3 directionToCursor = (boardPosition - cameraPosition).normalized;
        float targetCardHeight = boardPosition.y + cardOffset.y;

        if (Mathf.Abs(directionToCursor.y) > 0.001f) {
            float distanceAlongRay = (targetCardHeight - cameraPosition.y) / directionToCursor.y;
            if (distanceAlongRay > 0)
                return cameraPosition + directionToCursor * distanceAlongRay;
        }

        return new Vector3(boardPosition.x, targetCardHeight, boardPosition.z);
    }

    public void UpdateHoverStatus(TargetValidationState state) {
        
    }
}