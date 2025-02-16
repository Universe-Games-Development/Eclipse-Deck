using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Card Self Drawn", menuName = "Abilities/CardAbilities/Self Drawn")]
public class CardDrawnAbilitySO : CardAbilityData {
    string description = "When player draw this card ability activate";

    public override Ability GenerateAbility(IAbilitiesCaster owner, GameEventBus eventBus) {
        return new CardDrawnSelfAbility(description, this, owner, eventBus);
    }
}

// When player draw this card ability activate
public class CardDrawnSelfAbility : CardPassiveAbility {
    private string description;

    public CardDrawnSelfAbility(string description, CardAbilityData abilityData, IAbilitiesCaster card, GameEventBus eventBus) : base(abilityData, card, eventBus) {
        this.description = description;
    }

    private void OnCardDrawn(Card card) {
        DrawCardAbility();
    }

    public void DrawCardAbility() {
        if (Card.Data != null)
            Debug.Log($"Card {Card.Data.Name} drawn ability ACtivation for state : {Card.CurrentState}");
        Debug.Log(description);

    }
    public override void RegisterTrigger() {
        Card.OnCardDrawn += OnCardDrawn;
    }

    public override void DeregisterTrigger() {
        Card.OnCardDrawn -= OnCardDrawn;
    }
}