using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Card Self Drawn", menuName = "Abilities/CardAbilities/Self Drawn")]
public class CardDrawnAbilitySO : AbilitySO {
    public override Ability GenerateAbility(IAbilityOwner owner, GameEventBus eventBus) {
        return new CardDrawnSelfAbility(this, owner, eventBus);
    }
}

// When player draw this card ability activate
public class CardDrawnSelfAbility : Ability {
    private Card card;

    public CardDrawnSelfAbility(AbilitySO abilityData, IAbilityOwner abilityOwner, GameEventBus eventBus) : base(abilityData, abilityOwner, eventBus) {
        TrySetCardOwner(abilityOwner);
    }

    private void TrySetCardOwner(IAbilityOwner abilityOwner) {
        if (abilityOwner is Card card) {
            this.card = card;
        }
    }

    public override void Register() {
        if (card == null) {
            TrySetCardOwner(abilityOwner);
            if (card == null) return;
        }
        card.OnCardDrawn += AbilityActivation;
    }

    private void AbilityActivation(Card card) {
        if (card.Data != null)
            Debug.Log($"Card {card.Data.Name} drawn ability ACtivation for state : {card.CurrentState}");
    }

    public override void Deregister() {
        card.OnCardDrawn -= AbilityActivation;
    }
}