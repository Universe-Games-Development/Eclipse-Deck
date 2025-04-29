using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using Zenject;

public class BoardPlayer : MonoBehaviour, IGameUnit, IDamageable, IMannable {
    [Inject] private TurnManager _turnManager;
    [Inject] protected GameEventBus _eventBus;
    public Direction FacingDirection;
    [SerializeField] private HealthCellView _healthDisplay;
    [SerializeField] private CardsHandleSystem _cardsSystem;
    public Opponent Info { get; private set; }
    public Health Health { get; private set; }
    public Mana Mana { get; private set; }
    public EffectManager Effects { get; private set; }
    public ITargetingService TargetService { get; internal set; }

    //IGameUnit
    public BoardPlayer ControlledBy { get; private set; }
    public event Action<GameEnterEvent> OnUnitDeployed;

    /// <summary>
    /// Прив'язує об'єкт опонента до цього представлення на дошці
    /// </summary>
    public void BindPlayer(Opponent player) {
        if (player == null) {
            return;
        }
        Info = player;
        OpponentData Data = Info.Data;

        ControlledBy = this;
        Health = new Health(Data.Health, this, _eventBus);
        Mana = new Mana(this, Data.Mana);

        if (_healthDisplay != null) {
            _healthDisplay.Initialize();
            _healthDisplay.AssignOwner(this);
        }

        Effects = new EffectManager(_turnManager);
        OnUnitDeployed?.Invoke(new GameEnterEvent(this));
        InitializeCards();
    }

    /// <summary>
    /// Ініціалізує систему карт для гравця
    /// </summary>
    public void InitializeCards() {
        _cardsSystem.Initialize(ControlledBy);
        _cardsSystem.BattleStartAction();
    }

    public void DrawTestCards() {
        _cardsSystem.DrawCards(5);
    }

    /// <summary>
    /// Очищає гравця з позиції за дошкою
    /// </summary>
    public void SelfClear() {
        ControlledBy = null;

        if (_healthDisplay != null) {
            _healthDisplay.ClearOwner();
        }
    }

    private void OnDrawGizmosSelected() {
        Gizmos.DrawSphere(transform.position, 1f);
    }

    /// <summary>
    /// Отримує сервіс таргетингу для вказаного гравця
    /// </summary>
    public ITargetingService GetActionTargeting(Opponent player) {
        throw new NotImplementedException();
    }

    public override string ToString() {
        return $"{GetType().Name} {Info.Data.Name} ({Health.CurrentValue}/{Health.TotalValue})";
    }

    private async UniTask EndOwnTurn() {
        await UniTask.Delay(1500);
        _turnManager.EndTurnRequest(this);
    }
}