using UnityEngine;

[CreateAssetMenu(fileName = "Card Self Drawn", menuName = "Abilities/CardAbilities/Self Drawn")]
public class CardDrawnAbilitySO : CardAbilityData {
    public string testMessage = "When player draw this card ability activate";

    public override Ability<CardAbilityData, Card> CreateAbility(Card castingCard, GameEventBus eventBus) {
        return new CardDrawnSelfAbility(testMessage, castingCard, this, eventBus);
    }
}

// When player draw this card ability activate
public class CardDrawnSelfAbility : CardPassiveAbility {
    private string testMessage;

    public CardDrawnSelfAbility(string testMessage, Card card, CardDrawnAbilitySO abilityData, GameEventBus eventBus) : base(abilityData, card, eventBus) {
        this.testMessage = abilityData.testMessage;
    }
    
    private void OnCardDrawn(Card card) {
        if (card.Data != null)
            Debug.Log($"Card {card.Data.Name} drawn ability ACtivation for state : {card.CurrentState}");
        Debug.Log(testMessage);
    }

    protected override void ActivateAbilityTriggers() {
        card.OnCardDrawn += OnCardDrawn;
    }

    protected override void DeactivateAbilityTriggers() {
        card.OnCardDrawn -= OnCardDrawn;
    }

    protected override bool CheckActivationConditions() {
        return true;
    }
}