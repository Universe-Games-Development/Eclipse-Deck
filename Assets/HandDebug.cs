using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class HandDebug : MonoBehaviour
{
    [SerializeField] HandPresenter handPresenter;

    [SerializeField] Button addCardButton;
    [SerializeField] int initialCards = 0;
    [SerializeField] TextMeshProUGUI selectedCard;

    [SerializeField] CreatureCardData cardData;
    [Inject] GameEventBus eventBus;

    private void Start() {
        addCardButton.onClick.AddListener(() => {
            AddCard();
        });

        handPresenter.CardHand.OnCardSelection += UpdateSelectedCard;

        for (int i = 0; i < initialCards; i++) {
            AddCard();
        }
    }

    private void UpdateSelectedCard(Card card) {
        selectedCard.text = "Sel. Card: " + card;
    }

    private void AddCard() {
        EffectManager effectManager = new EffectManager(eventBus);
        Card card = new CreatureCard(cardData);
        handPresenter.CardHand.AddCard(card);
    }
}
