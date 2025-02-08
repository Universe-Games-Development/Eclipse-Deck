using UnityEngine;

public class CardAbilityPool : BasePool<CardAbilityUI> {
    public CardAbilityPool(CardAbilityUI abilityPrefab, Transform defaultParent)
        : base(abilityPrefab, defaultParent) {
    }
}