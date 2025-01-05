using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class CardAbility : IEventListener {
    private CardAbilitySO abilitySO;
    private Card card;
    private IEventManager eventManager;
    public CardState activationState;
    private bool isRegistered = false; // ��������� ��� ���������� ���������

    public CardAbility(CardAbilitySO abilitySO, Card card, IEventManager eventManager) {
        this.abilitySO = abilitySO;
        this.card = card;
        this.eventManager = eventManager;
        activationState = abilitySO.activationState;

        // ϳ������ �� ���� ���� ����� �����
        card.OnStateChanged += OnCardStateChanged;

        // �������� ����� ��� �����������
        CheckAndRegisterAbility();

        Debug.Log($"CardAbility created for card: {card.Name} in state: {card.CurrentState}");
    }

    // ���������� ��� ������� �� ��䳿 ��� ������� ��'����
    ~CardAbility() {
        card.OnStateChanged -= OnCardStateChanged;
        UnregisterActivation();
    }

    // ������� ���� ����� ������
    private void OnCardStateChanged(CardState newState) {
        Debug.Log($"Card state changed from {card.CurrentState} to {newState} for card: {card.Name}");
        CheckAndRegisterAbility();
    }

    // �������� � ��������� �������, ���� ���� ��������
    private void CheckAndRegisterAbility() {
        if (card.CurrentState == activationState) {
            if (!isRegistered) {
                RegisterActivation();
            }
        } else {
            if (isRegistered) {
                UnregisterActivation();
            }
        }
    }

    // ��������� �������
    public void RegisterActivation() {
        Debug.Log($"Registering ability for card: {card.Name}");
        eventManager.RegisterListener(this, EventType.ON_CARD_CLICKED, ExecutionType.Parallel);
        isRegistered = true;
    }

    // ³������ �� �������
    public void UnregisterActivation() {
        Debug.Log($"Unregistering ability for card: {card.Name}");
        eventManager.UnregisterListener(this, EventType.ON_CARD_CLICKED);
        isRegistered = false;
    }

    // ������� ��䳿
    public async UniTask OnEventAsync(EventType eventType, GameContext gameContext, CancellationToken cancellationToken = default) {
        if (!isRegistered) return; // �������� ����� ����������

        Debug.Log($"Event received: {eventType} for card: {card.Name}");

        try {
            // ��������� ActivateAbility � CardAbilitySO
            abilitySO.ActivateAbility(gameContext);  // ������ ������ ActivateAbility

            await UniTask.Delay(1000, cancellationToken: cancellationToken);

            if (cancellationToken.IsCancellationRequested) {
                Debug.Log("Event handling was cancelled.");
                return;
            }

            Debug.Log("Ability executed.");
        } catch (Exception e) {
            Debug.LogError($"Error during ability execution for card {card.Name}: {e.Message}\n{e.StackTrace}");
        }
    }

}
