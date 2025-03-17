using Cysharp.Threading.Tasks;
using System;
using System.ComponentModel;
using System.Threading;
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
    public Card(CardData cardSO, GameEventBus eventBus)  // Add owner to constructor
    {
        Data = cardSO;
        Id = Guid.NewGuid().ToString();
        this.eventBus = eventBus;
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
    
    public abstract UniTask<bool> PlayCard(Opponent cardPlayer, GameBoardController boardController, CancellationToken ct = default);

    internal void Deselect() {
        Debug.LogError("Deselect");
    }

    internal void Select() {
        Debug.LogError("Select");
    }

    public virtual void SetOwner(Opponent owner) {
        Owner = owner;
    }
}

public class SpellCard : Card {

    public SpellCard(SpellCardSO cardSO, GameEventBus eventBus)
        : base(cardSO, eventBus) {
    }

    
    public override async UniTask<bool> PlayCard(Opponent cardPlayer, GameBoardController boardController, CancellationToken ct = default) {
        Debug.Log("Spell card played");
        await UniTask.CompletedTask;
        return false;
    }
}

public class SupportCard : Card {
    public SupportCard(SupportCardSO cardSO, GameEventBus eventBus)
        : base(cardSO, eventBus) { }

    public override async UniTask<bool> PlayCard(Opponent cardPlayer, GameBoardController boardController, CancellationToken ct = default) {
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

    public CreatureCard(CreatureCardData cardSO, GameEventBus eventBus)
        : base(cardSO, eventBus) {
        creatureCardData = cardSO;
        Health = new(cardSO.MAX_CARD_HEALTH, cardSO.Health);
        Attack = new(cardSO.MAX_CARD_ATTACK, cardSO.Attack);
    }

    public override async UniTask<bool> PlayCard(Opponent summoner, GameBoardController boardController, CancellationToken ct = default) {
        Field field = await summoner.actionFiller.ProcessRequirementAsync(summoner, friendlyFieldRequirement, ct);
        if (field == null) {
            Debug.Log("Field is null");
            return false;
        }

        return await boardController.CreatureSpawner.SpawnCreature(this, field, summoner);
    }

    public override void SetOwner(Opponent newOwner) {
        base.SetOwner(newOwner);
        RequirementBuilder<Field> requirementBuilder = new();
        friendlyFieldRequirement = requirementBuilder
            .Add(new OwnerFieldRequirement(Owner))
            .Add(new EmptyFieldRequirement())
            .Build();
    }
}