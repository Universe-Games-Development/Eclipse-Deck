using System;
using UnityEngine;

public class CardMovementController : MonoBehaviour {
    [Header("Movement Settings")]
    [SerializeField] private Vector3 cardOffset = new Vector3(0f, 1.2f, 0f);

    [Header("Camera Ray")]
    [SerializeField] private bool useCameraRayPositioning = true;

    private CardPresenter currentCard;
    private Func<Vector3> getTargetPosition;
    private bool isMoving = false;

    private void Update() {
        if (isMoving && currentCard != null && getTargetPosition != null) {
            UpdateCardMovement();
        }
    }

    public void StartMovement(CardPresenter card, System.Func<Vector3> targetPositionGetter) {
        currentCard = card;
        getTargetPosition = targetPositionGetter;
        isMoving = true;
    }


    public void ForceStop() {
        currentCard.StopMovement();
        isMoving = false;
        currentCard = null;
        getTargetPosition = null;
    }

    private void UpdateCardMovement() {
        Vector3 currentPosition = currentCard.transform.position;
        Vector3 targetPosition = CalculateCardPosition();

        currentCard.StartPhysicsMovement(targetPosition);
    }

    private Vector3 CalculateCardPosition() {
        Vector3 boardPosition = getTargetPosition();

        if (!useCameraRayPositioning) {
            return boardPosition + cardOffset;
        }

        Camera mainCamera = Camera.main;
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
}