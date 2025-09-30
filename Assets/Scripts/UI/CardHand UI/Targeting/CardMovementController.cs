using UnityEngine;

public class CardMovementTargeting : MonoBehaviour, ITargetingVisualization {
    [Header("Movement Settings")]
    [SerializeField] private Vector3 cardOffset = new Vector3(0f, 1.2f, 0f);
    [SerializeField] private bool useCameraRayPositioning = true;
    [SerializeField] private int playRenderOrderBoost = 50;

    private CardPresenter cardPresenter;

    public void Initialize(CardPresenter presenter) {
        cardPresenter = presenter;
    }

    public void StartTargeting() {
        if (cardPresenter != null) {
            cardPresenter.ModifyRenderOrder(playRenderOrderBoost);
            cardPresenter.ToggleTiltMovement(true);
        }
    }

    public void UpdateTargeting(Vector3 cursorPosition) {
        if (cardPresenter != null) {
            Vector3 targetPosition = CalculateCardPosition(cursorPosition);
            cardPresenter.DoPhysicsMovement(targetPosition);
        }
    }

    public void StopTargeting() {
        if (cardPresenter != null) {
            cardPresenter.ModifyRenderOrder(-playRenderOrderBoost);
            cardPresenter.ToggleTiltMovement(false);
        }
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