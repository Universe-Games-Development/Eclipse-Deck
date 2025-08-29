using System;
using UnityEngine;

public class CardMovementController : MonoBehaviour, ITargetingVisualization {
    [Header("Movement Settings")]
    [SerializeField] private Vector3 cardOffset = new Vector3(0f, 1.2f, 0f);

    [Header("Camera Ray")]
    [SerializeField] private bool useCameraRayPositioning = true;

    private CardPresenter currentCard;
    private Func<Vector3> getTargetPosition;
    private bool isMoving = false;

    [SerializeField] private int playRenderOrderBoost = 50;

    #region ITargetingVisualization
    public void StartTargeting(Func<Vector3> targetPositionProvider, TargetSelectionRequest targetSelectionRequest) {
        if (targetPositionProvider == null) {
            Debug.LogError("Target position provider is null!");
            return;
        }

        if (targetSelectionRequest?.Initiator is not CardPresenter card) {
            Debug.LogError("Invalid initiator - must be CardPresenter");
            StopTargeting();
            return;
        }

        currentCard = card;
        currentCard.ModifyRenderOrder(playRenderOrderBoost);
        getTargetPosition = targetPositionProvider;
        isMoving = true;
    }

    public void UpdateTargeting() {
        if (isMoving && currentCard != null && getTargetPosition != null) {
            UpdateCardMovement();
        }
    }

    public void StopTargeting() {
        if (currentCard != null) {
            currentCard.ModifyRenderOrder(-playRenderOrderBoost);
            currentCard.StopMovement();
        }
        isMoving = false;
        currentCard = null;
        getTargetPosition = null;
    }
    #endregion


    private void UpdateCardMovement() {
        Vector3 currentPosition = currentCard.transform.position;
        Vector3 targetPosition = CalculateCardPosition();

        currentCard.DoPhysicsMovement(targetPosition);
    }

    private Vector3 CalculateCardPosition() {
        Vector3 boardPosition = getTargetPosition();

        if (!useCameraRayPositioning) {
            return boardPosition + cardOffset;
        }

        Camera mainCamera = GetMainCamera();
        if (mainCamera == null) {
            return boardPosition + cardOffset;
        }

        Vector3 cameraPosition = mainCamera.transform.position;
        Vector3 directionToCursor = (boardPosition - cameraPosition).normalized;
        float targetCardHeight = boardPosition.y + cardOffset.y;

        if (Mathf.Abs(directionToCursor.y) > 0.001f) {
            float distanceAlongRay = (targetCardHeight - cameraPosition.y) / directionToCursor.y;
            if (distanceAlongRay > 0) {
                return cameraPosition + directionToCursor * distanceAlongRay;
            }
        }

        return new Vector3(boardPosition.x, targetCardHeight, boardPosition.z);
    }


    private Camera GetMainCamera() {
        Camera mainCamera = Camera.main;
        if (mainCamera == null) {
            Debug.LogError("Main camera not found!");
            return null;
        }
        return mainCamera;
    }
}