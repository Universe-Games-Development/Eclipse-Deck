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

        tableManager.AssignFieldsToPlayer(this, 1);

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
        GameObject interactable = interactionManager.HoveredInteractable;
        if (interactable && interactable.TryGetComponent(out Field field)) {
            // Получаем выбранную карту напрямую из handUI
            CardUI selectedUICard = handUI.SelectedCard;
            Card selectedCard = hand.GetCardByID(selectedUICard.Id);

            if (selectedCard != null) {
                bool isPlayed = tableManager.SummonCreature(this, selectedCard, field);
                if (isPlayed) {
                    hand.RemoveCard(selectedCard);
                    field.SummonCreature(selectedCard);
                    handUI.DeselectCurrentCard();
                    Debug.Log($"Card {selectedCard.Name} summoned to field!");
                }
            } else {
                Debug.Log("No card selected to summon.");
            }
        } else {
            handUI.DeselectCurrentCard();
        }
    }
}
