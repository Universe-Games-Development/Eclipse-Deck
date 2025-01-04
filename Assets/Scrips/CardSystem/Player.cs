using UnityEngine;
using Zenject;

public class Player : Opponent {
    [SerializeField] private CardHandUI handUI;
    private InteractionManager interactionManager;

    [Inject]
    public void Construct(InteractionManager interactionManager) {
        this.interactionManager = interactionManager;
    }

    private void Start() {
        handUI.Initialize(hand);

        tableManager.AssignFieldsToPlayer(this, 0);

        hand.AddCard(deck.DrawCard());
        hand.AddCard(deck.DrawCard());
        hand.AddCard(deck.DrawCard());
        hand.AddCard(deck.DrawCard());
    }

    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            TrySummonCard();
        }
    }

    private void TrySummonCard() {
        // Проверяем, есть ли наведённый объект
        if (interactionManager.HoveredInteractable && interactionManager.HoveredInteractable.TryGetComponent(out Field field)) {
            // Получаем выбранную карту напрямую из handUI
            CardUI selectedUICard = handUI.SelectedCard;
            if (selectedUICard == null) {
                Debug.Log("No card selected to summon.");
                return;
            }

            Card selectedCard = hand.GetCardByID(selectedUICard.Id);
            if (selectedCard == null) {
                Debug.Log("Selected card not found in hand.");
                return;
            }

            bool isPlayed = tableManager.SummonCreature(this, selectedCard, field);
            if (isPlayed) {
                hand.RemoveCard(selectedCard);
                Debug.Log($"Card {selectedCard.Name} summoned to field!");
            }
        } else {
            handUI.DeselectCurrentCard();
        }
    }

}
