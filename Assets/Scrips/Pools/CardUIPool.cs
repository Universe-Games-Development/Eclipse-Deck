using UnityEngine;

public class CardUIPool : BasePool<CardUI> {
    private CardGhostPool ghostPool;
    private CardAbilityPool abilityPool;

    public CardUIPool(CardUI cardPrefab, CardGhostPool ghostPool, CardAbilityPool abilityPool, Transform defaultParent)
        : base(cardPrefab, defaultParent) {
        this.ghostPool = ghostPool;
        this.abilityPool = abilityPool;
    }

    protected override CardUI CreateObject() {
        CardUI cardUI = base.CreateObject();
        cardUI.SetAbilityPool(abilityPool);
        return cardUI;
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