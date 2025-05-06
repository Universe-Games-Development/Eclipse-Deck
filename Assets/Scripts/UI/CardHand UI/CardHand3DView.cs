using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class CardHand3DView : CardHandViewBase<Card3DView> {

    [SerializeField] private Card3DView cardPrefab;
    [SerializeField] private Transform cardsContainer;
    [SerializeField] private float cardSpacing = 1.2f;
    [SerializeField] private float cardCurveHeight = 0.5f;
    [SerializeField] private float cardRotationAngle = 5f;

    public override Card3DView BuildCardView(string id) {
        if (cardPrefab == null || cardsContainer == null) {
            Debug.LogError("CardPrefab or CardsContainer not set!", this);
            return null;
        }

        Card3DView card3D = Instantiate(cardPrefab, cardsContainer);
        UpdateCardPositions();
        return card3D;
    }

    public override void HandleCardViewRemoval(Card3DView card3D) {
        // Play removal animation before destroying
        card3D.RemoveCardView().Forget();

        // Update positions after removal
        UpdateCardPositions();
    }

    private void UpdateCardPositions() {
        if (_cardViews.Count == 0) return;

        int totalCards = _cardViews.Count;
        float centerOffset = (totalCards - 1) * cardSpacing / 2f;
        int index = 0;

        foreach (var card in _cardViews.Values) {
            if (card == null) continue;

            // Calculate position on a curve
            float xPos = (index * cardSpacing) - centerOffset;
            float yPos = CalculateYPosition(xPos, centerOffset);
            float zPos = 0f;

            // Calculate rotation (cards fan outward)
            float rotAngle = CalculateRotationAngle(xPos, centerOffset);

            // Set target position and rotation
            Vector3 targetPosition = new(xPos, yPos, zPos);
            Quaternion targetRotation = Quaternion.Euler(0, rotAngle, 0);

            // Animate the card to its position
            AnimateCardToPosition(card, targetPosition, targetRotation).Forget();

            index++;
        }
    }

    private float CalculateYPosition(float xPos, float maxOffset) {
        // Create a curve where center cards are lower than edge cards
        if (maxOffset == 0) return 0;
        float normalizedX = xPos / maxOffset;
        return -cardCurveHeight * (1 - Mathf.Abs(normalizedX));
    }

    private float CalculateRotationAngle(float xPos, float maxOffset) {
        // Create a fan effect where cards rotate outward
        if (maxOffset == 0) return 0;
        float normalizedX = xPos / maxOffset;
        return normalizedX * cardRotationAngle;
    }

    private async UniTask AnimateCardToPosition(Card3DView card, Vector3 targetPos, Quaternion targetRot) {
        await card.transform.DOLocalMove(targetPos, 0.3f).AsyncWaitForCompletion();
        await card.transform.DOLocalRotateQuaternion(targetRot, 0.2f).AsyncWaitForCompletion();
    }

    public override void Cleanup() {
        base.Cleanup();
    }
}