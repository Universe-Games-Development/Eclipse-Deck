using UnityEngine;

// This class creates CardUI object and assign ghost layout to it so it knows own target position
// 2 object pools for cards and it`s layout ghosts
public class CardUIPool : BasePool<CardUIView> {
    private CardGhostPool ghostPool;
    private CardAbilityPool abilityPool;

    public CardUIPool(CardUIView cardPrefab, CardGhostPool ghostPool, CardAbilityPool abilityPool, Transform defaultParent)
        : base(cardPrefab, defaultParent) {
        this.ghostPool = ghostPool;
        this.abilityPool = abilityPool;
    }

    protected override CardUIView CreateObject() {
        CardUIView cardUI = base.CreateObject();
        cardUI.Description.SetAbilityPool(abilityPool);
        return cardUI;
    }
    protected override void OnTakeFromPool(CardUIView card) {
        base.OnTakeFromPool(card);
    }

    protected override void OnReturnToPool(CardUIView card) {
        card.Reset();
        base.OnReturnToPool(card);
    }
}