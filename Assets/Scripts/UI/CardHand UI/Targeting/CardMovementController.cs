using System;
using UnityEngine;

public class CardMovementTargeting : MonoBehaviour, ITargetingVisualization {
    [Header("Movement Settings")]
    [SerializeField] private Vector3 cardOffset = new Vector3(0f, 1.2f, 0f);
    [SerializeField] private bool useCameraRayPositioning = true;
    [SerializeField] private int playRenderOrderBoost = 50;

    private CardPresenter card;

    public void Initialize(CardPresenter cardPresenter) {
        card = cardPresenter;
    }

    public void StartTargeting() {
        if (card != null) {
            card.SetInteractable(false);
            card.ModifyRenderOrder(playRenderOrderBoost);
        }
    }

    public void UpdateTargeting(Vector3 cursorPosition) {
        if (card != null) {
            Vector3 targetPosition = CalculateCardPosition(cursorPosition);
            card.DoPhysicsMovement(targetPosition);
        }
    }

    public void StopTargeting() {
        if (card != null) {
            card.SetInteractable(true);
            card.ModifyRenderOrder(-playRenderOrderBoost);
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
}