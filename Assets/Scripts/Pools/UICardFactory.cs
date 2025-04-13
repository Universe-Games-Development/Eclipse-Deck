using UnityEngine;

// This class creates CardUI object and assign ghost layout to it so it knows own target position
// 2 object pools for cards and it`s layout ghosts
public class CardUIPool : BasePool<CardView> {
    private CardGhostPool ghostPool;
    private CardAbilityPool abilityPool;

    public CardUIPool(CardView cardPrefab, CardGhostPool ghostPool, CardAbilityPool abilityPool, Transform defaultParent)
        : base(cardPrefab, defaultParent) {
        this.ghostPool = ghostPool;
        this.abilityPool = abilityPool;
    }

    protected override CardView CreateObject() {
        CardView cardUI = base.CreateObject();
        cardUI.Description.SetAbilityPool(abilityPool);
        return cardUI;
    }
    protected override void OnTakeFromPool(CardView card) {
        base.OnTakeFromPool(card);
    }

    protected override void OnReturnToPool(CardView card) {
        card.Reset();
        base.OnReturnToPool(card);
    }
}