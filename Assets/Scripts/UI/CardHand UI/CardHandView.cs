using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class CardHandView : MonoBehaviour {
    [SerializeField] protected HandLayoutStrategy layoutStrategy;
    [SerializeField] private float cardsOrganizeDuration = 0.2f;
    [SerializeField] protected float cardHoverDuration = 0.2f;
    [SerializeField] protected int baseRenderOrder = 2800;

    [Header("Hover Settings")]
    [SerializeField] protected float hoverOffsetY = 1.0f;
    [SerializeField] protected float hoverOffsetZ = 1.0f;
    [SerializeField] private int hoverRenderOrderBoost = 50;

    private CardView hoveredCard;
    private readonly Dictionary<CardView, CardPoint> cardLayoutData = new();

    public virtual void Toggle(bool value) => gameObject.SetActive(value);

    protected CardPoint[] GetCardPoints(int cardCount) =>
        layoutStrategy?.CalculateCardTransforms(cardCount) ?? new CardPoint[0];

    public virtual void UpdateCardPositions(List<CardView> cardViews) {
        if (cardViews == null) return;

        var points = GetCardPoints(cardViews.Count);

        // Очищуємо застарілі дані
        CleanupLayoutData(cardViews);

        for (int i = 0; i < points.Length && i < cardViews.Count; i++) {
            var cardPoint = points[i];
            var cardView = cardViews[i];

            if (cardView == null) continue;

            cardLayoutData[cardView] = points[i];

            // Анімуємо карту якщо вона не в hover стані
            AnimateToPosition(cardView, cardPoint, baseRenderOrder + i);
        }
    }

    public void UpdateSingleCardPosition(CardView cardView) {
        if (cardView == null || !cardLayoutData.TryGetValue(cardView, out var data)) {
            Debug.LogWarning($"No layout data found for card {cardView?.name}");
            return;
        }

        var cardPoint = new CardPoint { position = data.position, rotation = data.rotation };
        AnimateToPosition(cardView, cardPoint, data.sortingOrder);
    }

    private void AnimateToPosition(CardView cardView, CardPoint cardPoint, int renderOrder) {
        Transform cardTransform = cardView.transform;

        Tweener moveTween = cardTransform.DOMove(cardPoint.position, cardsOrganizeDuration)
                                    .SetEase(Ease.OutQuad)
                                    .SetLink(cardTransform.gameObject);

        cardView.DoTweener(moveTween);
        cardView.SetRenderOrder(renderOrder);
    }

    public virtual void SetCardHover(CardView cardView, bool isHovered) {
        if (cardView == null) return;

        if (isHovered) {
            SetHoveredCard(cardView);
        } else {
            ClearHoveredCard();
        }
    }

    private void SetHoveredCard(CardView cardView) {
        // Очищуємо попередню hover карту
        ClearHoveredCard();

        hoveredCard = cardView;

        if (!cardLayoutData.TryGetValue(cardView, out var data)) {
            Debug.LogWarning($"No layout data found for hovered card {cardView.name}");
            return;
        }

        // Розраховуємо hover позицію
        Vector3 hoverPosition = data.position + new Vector3(0f, hoverOffsetY, hoverOffsetZ);

        // Анімуємо до hover позиції
        Sequence hoverSequence = DOTween.Sequence();
        hoverSequence.Join(hoveredCard.transform.DOMove(hoverPosition, cardHoverDuration));
        hoverSequence.Join(hoveredCard.transform.DORotate(data.rotation.eulerAngles, cardHoverDuration));

        hoveredCard.DoSequence(hoverSequence);
        hoveredCard.ModifyRenderOrder(hoverRenderOrderBoost);
    }

    private void ClearHoveredCard() {
        if (hoveredCard == null) return;

        if (cardLayoutData.TryGetValue(hoveredCard, out var data)) {
            // Анімуємо назад до оригінальної позиції
            Sequence returnSequence = DOTween.Sequence();
            returnSequence.Join(hoveredCard.transform.DOMove(data.position, cardHoverDuration));
            returnSequence.Join(hoveredCard.transform.DORotate(data.rotation.eulerAngles, cardHoverDuration));

            hoveredCard.DoSequence(returnSequence);
        }

        hoveredCard.ModifyRenderOrder(-hoverRenderOrderBoost);
        hoveredCard = null;
    }

    

    private void CleanupLayoutData(List<CardView> activeCardViews) {
        // Видаляємо дані для карт, яких більше немає в активному списку
        var activeCardViewsSet = new HashSet<CardView>(activeCardViews);
        var keysToRemove = cardLayoutData.Keys.Where(k => !activeCardViewsSet.Contains(k)).ToList();

        foreach (var key in keysToRemove) {
            cardLayoutData.Remove(key);
        }
    }

    // Викликається коли карта видаляється з руки
    public virtual void RemoveCardView(CardView cardView) {
        if (cardView == null) return;

        // Очищуємо hover якщо це ця карта
        if (hoveredCard == cardView) {
            hoveredCard = null;
        }

        // Видаляємо дані макету
        cardLayoutData.Remove(cardView);

        // Дозволяємо наслідникам вирішити що робити з вʼю
        HandleCardViewRemoval(cardView);
    }

    // Абстрактний метод для обробки видалення - наслідники вирішують чи знищувати об'єкт
    protected abstract void HandleCardViewRemoval(CardView cardView);

    // Метод для отримання оригінальної позиції карти (для налагодження)
    public Vector3? GetOriginalCardPosition(CardView cardView) {
        return cardLayoutData.TryGetValue(cardView, out var data)
            ? data.position
            : null;
    }

    protected virtual void OnDestroy() {
        cardLayoutData.Clear();
        hoveredCard = null;
    }

    // Абстрактні методи для створення та знищення карт
    public abstract CardView CreateCardView(Card card);
}
public abstract class HandLayoutStrategy : MonoBehaviour {
    public abstract CardPoint[] CalculateCardTransforms(int cardCount);
}

[System.Serializable]
public struct CardPoint {
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;
    public int sortingOrder;

    public CardPoint(Vector3 pos, Quaternion rot, Vector3 scl, int sorting = 0) {
        position = pos;
        rotation = rot;
        scale = scl;
        sortingOrder = sorting;
    }
}


