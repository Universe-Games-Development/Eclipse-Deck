using Cysharp.Threading.Tasks;
using System;
using System.ComponentModel;
using UnityEngine;

public abstract class Card : IAbilityOwner {
    public event Action<Card> OnCardDrawn;
    public event Action<Card> OnCardShuffled;
    public event Action<Card> OnCardRDiscarded;
    public Action<CardState> OnStateChanged { get; internal set; }
    public CardState CurrentState { get; protected set; }

    public string Id { get; protected set; }
    public CardData Data { get; protected set; }
    public Cost Cost { get; protected set; }
    public GameEventBus eventBus { get; protected set; }
    
    public Opponent Owner { get; protected set; } // Add Owner here

    public CardUI cardUI;
    public AbilityManager<CardAbilityData, Card> _abilityManager;
    public Card(CardData cardSO, Opponent owner, GameEventBus eventBus)  // Add owner to constructor
    {
        Data = cardSO;
        Id = Guid.NewGuid().ToString();
        this.eventBus = eventBus;
        Owner = owner; // Assign the owner
        Cost = new Cost(cardSO.MAX_CARDS_COST, cardSO.cost);

        _abilityManager = new AbilityManager<CardAbilityData, Card>(this, eventBus);
        _abilityManager.AddAbilities(cardSO.abilities);
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
    
    public abstract UniTask<bool> PlayCard(Opponent cardPlayer, GameBoardController boardController);

    internal void Deselect() {
        Debug.LogError("Deselect");
    }

    internal void Select() {
        Debug.LogError("Select");
    }
}

public class SpellCard : Card {

    public SpellCard(SpellCardSO cardSO, Opponent owner, GameEventBus eventBus)
        : base(cardSO, owner, eventBus) {
    }

    
    public override async UniTask<bool> PlayCard(Opponent cardPlayer, GameBoardController boardController) {
        Debug.Log("Spell card played");
        await UniTask.CompletedTask;
        return false;
    }
}

public class SupportCard : Card {
    public SupportCard(SupportCardSO cardSO, Opponent owner, GameEventBus eventBus)
        : base(cardSO, owner, eventBus) { }

    public override async UniTask<bool> PlayCard(Opponent cardPlayer, GameBoardController boardController) {
        Debug.Log("Support card played");
        await UniTask.CompletedTask;
        return false;
    }
}

public class CreatureCard : Card {
    public Stat Health { get; private set; }
    public Stat Attack { get; private set; }
    public CreatureCardData creatureCardData;
    private IRequirement<Field> friendlyFieldRequirement;

    public CreatureCard(CreatureCardData cardSO, Opponent owner, GameEventBus eventBus)
        : base(cardSO, owner, eventBus) {
        creatureCardData = cardSO;
        Health = new(cardSO.MAX_CARD_HEALTH, cardSO.Health);
        Attack = new(cardSO.MAX_CARD_ATTACK, cardSO.Attack);

        RequirementBuilder<Field> requirementBuilder = new RequirementBuilder<Field>();
        friendlyFieldRequirement = requirementBuilder
            .Add(new OwnerFieldRequirement(owner))
            .Add(new EmptyFieldRequirement())
            .Build();
    }

    public override async UniTask<bool> PlayCard(Opponent summoner, GameBoardController boardController) {
        Field field = await summoner.actionFiller.ProcessRequirementAsync(summoner, friendlyFieldRequirement);
        if (field == null) {
            Debug.Log("Field is null");
            return false;
        }

        return await boardController.CreatureSpawner.SpawnCreature(this, field, summoner);
    }
}