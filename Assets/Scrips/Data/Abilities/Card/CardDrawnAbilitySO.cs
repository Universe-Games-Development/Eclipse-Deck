using UnityEngine;
using static CardAbility;

[CreateAssetMenu(fileName = "Card Self Drawn", menuName = "Abilities/CardAbilities/Self Drawn")]
public class CardDrawnAbilitySO : CardAbilityData {
    public string testMessage = "When player draw this card ability activate";

    public override CardAbility GenerateAbility(Card castingCard, GameEventBus eventBus) {
        return new CardDrawnSelfAbility(castingCard, this, eventBus);
    }
}

// When player draw this card ability activate
public class CardDrawnSelfAbility : PassiveCardAbility {
    private string testMessage;

    public CardDrawnSelfAbility(Card card, CardDrawnAbilitySO abilityData, GameEventBus eventBus) : base(card, abilityData, eventBus) {
        this.testMessage = abilityData.testMessage;
        AbilityData = abilityData;
        castingCard = card;
    }
    
    private void OnCardDrawn(Card card) {
        if (castingCard.Data != null)
            Debug.Log($"Card {castingCard.Data.Name} drawn ability ACtivation for state : {castingCard.CurrentState}");
        Debug.Log(testMessage);
    }

    public override bool ActivationCondition() {
        return true;
    }

    public override void RegisterTrigger() {
        castingCard.OnCardDrawn += OnCardDrawn;
    }

    public override void DeregisterTrigger() {
        castingCard.OnCardDrawn -= OnCardDrawn;
    }
}