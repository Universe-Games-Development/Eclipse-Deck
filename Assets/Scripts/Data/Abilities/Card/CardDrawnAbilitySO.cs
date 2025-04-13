using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "Card Self Drawn", menuName = "Abilities/CardAbilities/Self Drawn")]
public class CardDrawnAbilitySO : CardAbilityData {
    public string testMessage = "When player draw this card ability activate";

    public override Ability<CardAbilityData, Card> CreateAbility(Card castingCard, DiContainer container) {
        return container.Instantiate<CardDrawnSelfAbility>(new object[] { this, castingCard, testMessage }); ;
    }
}

// When player draw this card ability activate
public class CardDrawnSelfAbility : CardPassiveAbility {
    private string testMessage;

    public CardDrawnSelfAbility(CardAbilityData data, Card owner, string message) : base(data, owner) {
        testMessage = message;
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