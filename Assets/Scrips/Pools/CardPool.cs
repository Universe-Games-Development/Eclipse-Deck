using UnityEngine;

public class CardPool : BasePool<CardUI> {
    private CardGhostPool ghostPool;

    public CardPool(CardUI cardPrefab, CardGhostPool ghostPool, Transform defaultParent)
        : base(cardPrefab, defaultParent) {
        this.ghostPool = ghostPool;
    }

    protected override void OnTakeFromPool(CardUI card) {
        base.OnTakeFromPool(card);
        card._doAnimator.CardLayoutGhost = ghostPool.Get();;
    }

    protected override void OnReturnToPool(CardUI card) {
        ghostPool.Release(card._doAnimator.CardLayoutGhost);
        card.Reset();
        base.OnReturnToPool(card);
    }
}