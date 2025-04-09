using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using Zenject;
using static UnityEngine.UI.GridLayoutGroup;

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

    [Inject]
    public void Construct(GameEventBus eventBus) {
        this.eventBus = eventBus;
        _abilityManager = new AbilityManager<CardAbilityData, Card>(this, eventBus);
        _abilityManager.AddAbilities(Data.abilities);
    }

    public Card(CardData cardData, Opponent owner)  // Add owner to constructor
    {
        Owner = owner;
        Data = cardData;
        Id = Guid.NewGuid().ToString();
        
        Cost = new Cost(cardData.MAX_CARDS_COST, cardData.cost);
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
    
    public abstract UniTask<bool> PlayCard(Opponent cardPlayer, BoardGame boardPresenter, CancellationToken ct = default);

    internal void Deselect() {
        Debug.LogError("Deselect");
    }

    internal void Select() {
        Debug.LogError("Select");
    }

    // States will be changed by subscribers (discrad deck / Deck / Exile Deck)
    public void Exile() {
        eventBus.Raise(new ExileCardEvent(this));
    }

    public void Discard() {
        eventBus.Raise(new DiscardCardEvent(this));
    }
}

public struct DiscardCardEvent : IEvent {
    public Card card;
    public DiscardCardEvent(Card card) {
        this.card = card;
    }
}

public struct ExileCardEvent : IEvent {
    public Card card;
    public ExileCardEvent(Card card) {
        this.card = card;
    }
}

public class SpellCard : Card {

    public SpellCard(SpellCardData cardData, Opponent owner)
        : base(cardData, owner) {
    }

    
    public override async UniTask<bool> PlayCard(Opponent cardPlayer, BoardGame boardController, CancellationToken ct = default) {
        Debug.Log("Spell card played");
        await UniTask.CompletedTask;
        return false;
    }
}

public class SupportCard : Card {
    public SupportCard(SupportCardData cardData, Opponent owner)
        : base(cardData, owner) { }

    public override async UniTask<bool> PlayCard(Opponent cardPlayer, BoardGame boardController, CancellationToken ct = default) {
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

    public CreatureCard(CreatureCardData cardData, Opponent owner)
        : base(cardData, owner) {
        creatureCardData = cardData;
        Health = new(cardData.Health);
        Attack = new(cardData.Attack);

        RequirementBuilder<Field> requirementBuilder = new();
        friendlyFieldRequirement = requirementBuilder
            .Add(new OwnerFieldRequirement(Owner))
            .Add(new EmptyFieldRequirement())
            .Build();
    }

    public override async UniTask<bool> PlayCard(Opponent summoner, BoardGame boardPresenter, CancellationToken ct = default) {
        Field field = await summoner.actionFiller.ProcessRequirementAsync(summoner, friendlyFieldRequirement, ct);
        if (field == null) {
            Debug.Log("Field is null");
            return false;
        }

        return boardPresenter.SpawnCreature(this, field, summoner);
    }
}