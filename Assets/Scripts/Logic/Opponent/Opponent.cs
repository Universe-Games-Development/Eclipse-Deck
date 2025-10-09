using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class Opponent : UnitModel, IHealthable, IManaSystem, IDisposable {
    public override string OwnerId {
        get { return Id; }
    }

    public Action<Card, CardPlayResult> OnCardPlayFinished;
    public Action<Card> OnCardPlayStarted;

    public Health Health { get; private set; }
    public Mana Mana { get; private set; }
    public OpponentData Data { get; private set; }

    public CardSpendable CardSpendable { get; private set; }
    public Deck Deck { get; private set; }
    public CardHand Hand { get; private set; }

    private ICardPlayService cardPlayService;
    private IEventBus<IEvent> eventBus;

    public Opponent(OpponentData data, Deck deck, CardHand hand, ICardPlayService cardPlayService, IEventBus<IEvent> eventBus) {
        Data = data ?? throw new ArgumentNullException(nameof(data));
        Deck = deck ?? throw new ArgumentNullException(nameof(deck));
        Hand = hand ?? throw new ArgumentNullException(nameof(hand));

        Id = $"{Data.Name}_{Guid.NewGuid().ToString("N").Substring(0, 8)}";

        Health = new Health(data.Health);
        Mana = new Mana(data.Mana);
        CardSpendable = new CardSpendable(Mana, Health);

        
        Deck.ChangeOwner(Id);
        Hand.ChangeOwner(Id);

        this.cardPlayService = cardPlayService;
        this.eventBus = eventBus;
        cardPlayService.OnCardPlayFinished += HandleCardPlayFinished;
    }

    private void HandleCardPlayFinished(Card card, CardPlayResult result) {
        if (result.IsSuccess && Hand.Contains(card)) {
            Hand.Remove(card);
        }

        OnCardPlayFinished?.Invoke(card, result);
    }

    public void SpendMana(int currentValue) {
        int was = Mana.Current;
        Mana.Subtract(currentValue);
        //Debug.Log($"Mana: {Mana.Current} / {Mana.Max}");
    }

    public void PlayCard(Card card) {
        if (card == null) return;
        cardPlayService.PlayCardAsync(card);
        OnCardPlayStarted?.Invoke(card);
    }

    #region IHealthable
    public bool IsDead => Health.IsDead;

    public int CurrentHealth => Health.Current;

    public void TakeDamage(int damage) {
        Health.TakeDamage(damage);
    }
    #endregion

    public void DrawCard() {
        Card card = Deck.Draw();
        if (card == null) {
            // Handle empty deck logic
            TakeDamage(1);
            return;
        }
        Hand.Add(card);
    }


    public void RestoreMana() {
        Mana.RestoreMana();
    }

    public void EndTurn() {
        eventBus.Raise(new TurnEndEvent(this));
    }

    public void Dispose() {
        cardPlayService.OnCardPlayFinished -= HandleCardPlayFinished;
    }
}



public interface IOpponentController {
    void StartTurn();
    void EndTurn();
    UniTask<UnitModel> SelectTargetAsync(TargetSelectionRequest selectionRequst, CancellationToken cancellationToken);
}

public interface IInputService {
    event Action<Card> OnCardSelected;
    event Action OnEndTurnClicked;
}

public class PlayerController : IOpponentController, IDisposable {
    private readonly Opponent player;
    private readonly ITargetSelectionService selectionService;
    private readonly IInputService inputService;

    private PlayerState currentState;
    private PlayerState previousState;

    public PlayerController(
        Opponent player,
        ITargetSelectionService selectionService,
        IInputService inputService) // Додаємо в конструктор
    {
        this.player = player ?? throw new ArgumentNullException(nameof(player));
        this.selectionService = selectionService ?? throw new ArgumentNullException(nameof(selectionService));
        this.inputService = inputService ?? throw new ArgumentNullException(nameof(inputService));

        inputService.OnEndTurnClicked += EndTurn;

        // Початковий стан
        SwitchState(new IdleState());
    }

    public void StartTurn() {
        player.RestoreMana();
        player.DrawCard();
        SwitchState(new IdleState());
    }

    public void EndTurn() {
        SwitchState(new PassiveState());
        player.EndTurn();
    }

    public void PlayCard(Card card) {
        player.PlayCard(card);
    }

    public async UniTask<UnitModel> SelectTargetAsync(
        TargetSelectionRequest selectionRequest,
        CancellationToken cancellationToken) {
        SwitchState(new BusyState());
        UnitModel unitModel = await selectionService.SelectTargetAsync(selectionRequest, cancellationToken);
        ReturnToPreviousState();
        return unitModel;
    }

    public void SwitchState(PlayerState newState) {
        if (newState == null) {
            Debug.LogError("Attempting to switch to null state");
            return;
        }

        if (currentState?.GetType() == newState.GetType()) {
            return;
        }
        previousState = currentState ?? new PassiveState();
        currentState?.Exit();

        currentState = newState;
        currentState.Initialize(this, inputService, selectionService); // Передаємо залежності
        currentState.Enter();

        Debug.Log($"State changed to: {currentState.GetType().Name}");
    }

    public void ReturnToPreviousState() {
        if (previousState != null) {
            SwitchState(previousState);
        } else {
            SwitchState(new IdleState());
        }
    }

    public void Dispose() {
        currentState?.Exit();
        inputService.OnEndTurnClicked -= EndTurn;
    }
}

public abstract class PlayerState : State {
    protected PlayerController Controller { get; private set; }
    protected IInputService InputService { get; private set; }
    protected ITargetSelectionService SelectionService { get; private set; }

    public void Initialize(
    PlayerController controller,
    IInputService inputService,
    ITargetSelectionService selectionService) {
        Controller = controller ?? throw new ArgumentNullException(nameof(controller));
        InputService = inputService ?? throw new ArgumentNullException(nameof(inputService));
        SelectionService = selectionService ?? throw new ArgumentNullException(nameof(selectionService));
    }
}

public class PassiveState : PlayerState {
    // Гравець не може нічого робити (не його хід)
    public override void Enter() {
        base.Enter();
        Debug.Log("Player is in passive state (not their turn)");
    }

    public override void Exit() {
        base.Exit();
    }
}

public class IdleState : PlayerState {
    public override void Enter() {
        base.Enter();
        InputService.OnCardSelected += HandleCardSelection;
        Debug.Log("Player can now play cards");
    }

    private void HandleCardSelection(Card card) {
        if (card == null) return;
        Controller.PlayCard(card);
    }

    public override void Exit() {
        base.Exit();
        InputService.OnCardSelected -= HandleCardSelection;
    }
}

public class BusyState : PlayerState {
    public override void Enter() {
        base.Enter();
        Debug.Log("Player is busy selecting target");
    }

    public override void Exit() {
        base.Exit();
    }
}
