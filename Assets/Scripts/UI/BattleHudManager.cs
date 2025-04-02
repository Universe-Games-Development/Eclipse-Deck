using TMPro;
using UnityEngine;
using Zenject;

public class CardBattleInfo : MonoBehaviour {
    [Header("References")]
    [SerializeField] private GameObject hudRoot;
    [SerializeField] private TMP_Text turnCounterText;
    [SerializeField] private TMP_Text roundCounterText;
    [SerializeField] private TMP_Text activeOpponentText;

    [Header("Settings")]
    [SerializeField] private string turnTextFormat = "Turn: {0}";
    [SerializeField] private string roundTextFormat = "Round: {0}";
    [SerializeField] private string activeOpponentFormat = "Current Player: {0}";

    private TurnManager turnManager;
    private BattleManager battleManager;
    private GameEventBus eventBus;
    public bool isInitialized = false;

    [Inject]
    public void Construct(GameEventBus eventBus, [InjectOptional] TurnManager turnManager, [InjectOptional] BattleManager battleManager) {
        this.eventBus = eventBus;
        this.turnManager = turnManager;
        this.battleManager = battleManager;

        // ��������� HUD �� ������� ���
        SetHUDVisible(false);

        // ϳ��������� �� ��䳿
        SubscribeToEvents();
    }

    private void OnDestroy() {
        UnsubscribeFromEvents();
    }

    private void SubscribeToEvents() {
        // ϳ��������� �� ��䳿 GameEventBus
        if (eventBus != null) {
            eventBus.SubscribeTo<BattleStartedEvent>(OnBattleStarted);
            eventBus.SubscribeTo<BattleEndEventData>(OnBattleEnded);
            eventBus.SubscribeTo<OnTurnStart>(OnTurnStarted);
            eventBus.SubscribeTo<OnRoundStart>(OnRoundStarted);
        }

        // ϳ��������� �� ��䳿 TurnManager, ���� �� ����
        if (turnManager != null) {
            turnManager.OnOpponentChanged += OnOpponentChanged;
        }
    }

    private void UnsubscribeFromEvents() {
        // ³��������� �� ���� GameEventBus
        if (eventBus != null) {
            eventBus.UnsubscribeFrom<BattleStartedEvent>(OnBattleStarted);
            eventBus.UnsubscribeFrom<BattleEndEventData>(OnBattleEnded);
            eventBus.UnsubscribeFrom<OnTurnStart>(OnTurnStarted);
            eventBus.UnsubscribeFrom<OnRoundStart>(OnRoundStarted);
        }

        // ³��������� �� ���� TurnManager, ���� �� ����
        if (turnManager != null) {
            turnManager.OnOpponentChanged -= OnOpponentChanged;
        }
    }

    private void OnBattleStarted(ref BattleStartedEvent eventData) {
        // ���������� HUD
        InitializeHUD();
        SetHUDVisible(true);
    }

    private void OnBattleEnded(ref BattleEndEventData eventData) {
        // ��������� HUD
        SetHUDVisible(false);
    }

    private void OnTurnStarted(ref OnTurnStart eventData) {
        UpdateTurnInfo(eventData.TurnNumber, eventData.StartingOpponent);
    }

    private void OnRoundStarted(ref OnRoundStart eventData) {
        UpdateRoundInfo(eventData.RoundNumber);
    }

    private void OnOpponentChanged(Opponent newOpponent) {
        // ��������� ���������� ��� ��������� ������
        if (activeOpponentText != null) {
            activeOpponentText.text = string.Format(activeOpponentFormat, newOpponent.Name);
        }
    }

    private void InitializeHUD() {
        // ���������� HUD � ��������� ������, ���� TurnManager ����
        if (turnManager != null) {
            UpdateTurnInfo(turnManager.TurnCounter, turnManager.ActiveOpponent);
        }

        isInitialized = true;
    }

    private void UpdateTurnInfo(int turnNumber, Opponent opponent) {
        if (turnCounterText != null) {
            turnCounterText.text = string.Format(turnTextFormat, turnNumber);
        }

        if (activeOpponentText != null && opponent != null) {
            activeOpponentText.text = string.Format(activeOpponentFormat, opponent.Name);
        }
    }

    private void UpdateRoundInfo(int roundNumber) {
        if (roundCounterText != null) {
            roundCounterText.text = string.Format(roundTextFormat, roundNumber);
        }
    }

    private void SetHUDVisible(bool visible) {
        if (hudRoot != null) {
            hudRoot.SetActive(visible);
        }
    }
}

// ���� ��� ��'����� BattleHUD ����� Zenject
public class BattleHUDInstaller : MonoInstaller {
    [SerializeField] private CardBattleInfo battleHUDPrefab;

    public override void InstallBindings() {
        if (battleHUDPrefab != null) {
            Container.Bind<CardBattleInfo>().FromComponentInNewPrefab(battleHUDPrefab).AsSingle().NonLazy();
        }
    }
}