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
    
    public abstract UniTask<bool> PlayCard(Opponent cardPlayer, IAbilityInputter abilityInputter);

    internal void Deselect() {
        Debug.LogError("Deselect");
    }

    internal void Select() {
        Debug.LogError("Select");
    }
}

public class SpellCard : Card, IAbilitiesCaster {

    public CardAbilityManager AbilityManager { get; protected set; }
    public SpellCard(SpellCardSO cardSO, Opponent owner, GameEventBus eventBus)
        : base(cardSO, owner, eventBus) {
        AbilityManager = new CardAbilityManager(this, eventBus);
        AbilityManager.InitializeAbilities(cardSO.abilities);
    }

    public CardAbilityManager GetAbilityManager() {
        return AbilityManager;
    }

    public override async UniTask<bool> PlayCard(Opponent cardPlayer, IAbilityInputter abilityInputter) {
        Debug.Log("Spell card played");
        await UniTask.CompletedTask;
        return false;
    }
}

public class SupportCard : Card {
    public SupportCard(SupportCardSO cardSO, Opponent owner, GameEventBus eventBus)
        : base(cardSO, owner, eventBus) { }

    public override async UniTask<bool> PlayCard(Opponent cardPlayer, IAbilityInputter abilityInputter) {
        Debug.Log("Support card played");
        await UniTask.CompletedTask;
        return false;
    }
}

public class CreatureCard : Card {
    public Stat Health { get; private set; }
    public Stat Attack { get; private set; }
    
    public CreatureCard(CreatureCardSO cardSO, Opponent owner, GameEventBus eventBus)
        : base(cardSO, owner, eventBus) {
        
        Health = new(cardSO.MAX_CARD_HEALTH, cardSO.Health);
        Attack = new(cardSO.MAX_CARD_ATTACK, cardSO.Attack);
    }

    public override async UniTask<bool> PlayCard(Opponent cardPlayer, IAbilityInputter abilityInputter) {
        RequirementBuilder<Field> requirementBuilder = new RequirementBuilder<Field>();
        IRequirement<Field> friendlyFieldRequirement = requirementBuilder.Add(new OwnerFieldRequirement(cardPlayer))
            .Build();


        Field field = await abilityInputter.ProcessRequirementAsync(cardPlayer, friendlyFieldRequirement);
        if (field == null) {
            Debug.Log("Field is null");
            return false;
        }

        if (!field.IsSommonable(cardPlayer)) {
            await UniTask.FromCanceled();
            return false;
        }

        Creature creature = new Creature(this, cardPlayer, eventBus);

        return await field.SummonCreatureAsync(creature, cardPlayer);
    }
}