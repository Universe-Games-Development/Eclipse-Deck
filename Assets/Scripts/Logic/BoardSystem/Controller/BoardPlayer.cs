using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using Zenject;

public class BoardPlayer : UnitPresenter, IHealthable, IMannable {
    [Inject] protected IEventBus<IEvent> _eventBus;
    public Direction FacingDirection;
    [SerializeField] private HealthCellView _healthDisplay;
    [SerializeField] private CardsHandleSystem _cardsSystem;
    public Character Character { get; private set; }
    [Header("Debug")]
    [SerializeField] public CharacterData Data;
    public Health Health { get; private set; }
    public Mana Mana { get; private set; }
    public EffectManager EffectManager { get; private set; }
    public HumanTargetSelector Selector;

    private void Awake() {
        if (Data != null) {
            Character = new Character(Data);
        }
    }
    /// <summary>
    /// Прив'язує об'єкт опонента до цього представлення на дошці
    /// </summary>
    public void BindPlayer(CharacterPresenter characterPresenter) {
        if (characterPresenter == null) {
            return;
        }

        Character = characterPresenter.Model;
        CharacterData Data = Character.Data;

        Health = new Health(Data.Health, this);
        Mana = new Mana(this, Data.Mana);

        if (_healthDisplay != null) {
            _healthDisplay.Initialize();
            _healthDisplay.AssignOwner(this);
        }

        EffectManager = new EffectManager(_eventBus);
        InitializeCards();
    }

    /// <summary>
    /// Ініціалізує систему карт для гравця
    /// </summary>
    public void InitializeCards() {
        _cardsSystem.Initialize(this);


        BattleStartedEvent battleStartedEvent = new BattleStartedEvent();
        _cardsSystem.StartBattleActions(ref battleStartedEvent);
    }

    public void DrawTestCards() {
        _cardsSystem.DrawCards(5);
    }

    /// <summary>
    /// Очищає гравця з позиції за дошкою
    /// </summary>
    public void SelfClear() {

        if (_healthDisplay != null) {
            _healthDisplay.ClearOwner();
        }
    }

    private void OnDrawGizmosSelected() {
        Gizmos.DrawSphere(transform.position, 1f);
    }

    #region Unit presenter API
    public override UnitModel GetModel() {
        return Character;
    }

    public override BoardPlayer GetPlayer() {
        return this;
    }
    #endregion

    public void SpendMana(int currentValue) {
        int was = Mana.Current;
        Mana.Subtract(currentValue);
        DebugLog($"Mana: {Mana.Current} / {Mana.MinValue}");
    }

    public override string ToString() {
        return $"{GetType().Name} {Character.Data.Name} ({Health.Current}/{Health.TotalValue})";
    }
}