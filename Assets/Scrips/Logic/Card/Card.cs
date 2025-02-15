using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public abstract class Card {
    public event Action<Card> OnCardDrawn;
    public event Action<Card> OnCardShuffled;
    public event Action<Card> OnCardRDiscarded;
    public Action<CardState> OnStateChanged { get; internal set; }
    public CardState CurrentState { get; protected set; }

    public string Id { get; protected set; }
    public CardSO Data { get; protected set; }
    public Cost Cost { get; protected set; }
    public GameEventBus eventBus { get; protected set; }
    
    public Opponent Owner { get; protected set; } // Add Owner here

    public CardUI cardUI;
    public Card(CardSO cardSO, Opponent owner, GameEventBus eventBus)  // Add owner to constructor
    {
        Data = cardSO;
        Id = Guid.NewGuid().ToString();
        this.eventBus = eventBus;
        Owner = owner; // Assign the owner
        Cost = new Cost(cardSO.MAX_CARDS_COST, cardSO.cost);

        ChangeState(CardState.InDeck);
    }

    public virtual void ChangeState(CardState newState) {
        if (newState != CurrentState) {
            CurrentState = newState;
            switch (newState) {
                case CardState.InDeck:
                    OnCardShuffled?.Invoke(this);
                    break;
                case CardState.InHand:
                    OnCardDrawn?.Invoke(this);
                    break;
                case CardState.Discarded:
                    OnCardRDiscarded?.Invoke(this);
                    break;
                default:
                    throw new ArgumentException("Wrong new state");
            }
        }
    }
    
    public abstract UniTask<bool> PlayCard(Opponent cardPlayer, ICardsInputFiller cardsInputFiller);

    internal void Deselect() {
        Debug.LogError("Deselect");
    }

    internal void Select() {
        Debug.LogError("Select");
    }
}

public class SpellCard : Card, IAbilityOwner {
    public AbilityManager AbilityManager { get; protected set; }

    public SpellCard(SpellCardSO cardSO, Opponent owner, GameEventBus eventBus)
        : base(cardSO, owner, eventBus) {
        AbilityManager = new AbilityManager(this, eventBus);
        AbilityManager.InitializeAbilities(cardSO.abilities);
    }
    public AbilityManager GetAbilityManager() {
        return AbilityManager;
    }

    public override async UniTask<bool> PlayCard(Opponent cardPlayer, ICardsInputFiller cardsInputFiller) {
        Debug.Log("Spell card played");
        await UniTask.CompletedTask;
        return false;
    }
}

public class SupportCard : Card {
    public SupportCard(SupportCardSO cardSO, Opponent owner, GameEventBus eventBus)
        : base(cardSO, owner, eventBus) { }

    public override async UniTask<bool> PlayCard(Opponent cardPlayer, ICardsInputFiller cardsInputFiller) {
        Debug.Log("Support card played");
        await UniTask.CompletedTask;
        return false;
    }
}

public class CreatureCard : Card {
    public Stat Attack { get; private set; }
    public Stat HealthStat { get; private set; }

    public CreatureCard(CreatureCardSO cardSO, Opponent owner, GameEventBus eventBus)
        : base(cardSO, owner, eventBus) {
        Attack = new(cardSO.MAX_CARD_ATTACK, cardSO.Attack);
        HealthStat = new(cardSO.MAX_CARD_HEALTH, cardSO.Health);
    }

    public override async UniTask<bool> PlayCard(Opponent cardPlayer, ICardsInputFiller cardsInputFiller) {
        IInputRequirementRegistry cardInputRequirements = cardsInputFiller.GetRequirementRegistry();
        CardInputRequirement<FieldController> cardInputRequirement = cardInputRequirements.GetRequirement<FieldController>(typeof(FriendlyFieldInputRequirement));

        FieldController fieldToSummon = await cardsInputFiller.ProcessRequirementAsync<FieldController>(cardPlayer, cardInputRequirement);
        if (fieldToSummon == null) {
            return false;
        }
        Field field = fieldToSummon.Logic;
        if (field == null) {
            Debug.Log("Field is null");
            return false;
        }

        if (!field.IsSommonable(cardPlayer)) {
            await UniTask.FromCanceled();
        }

        Creature creature = new Creature(this, cardPlayer, eventBus);

        return await field.SummonCreatureAsync(creature, cardPlayer);
    }
}