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

        for (int i = 0; i < initialCards; i++) {
            AddCard();
        }
    }

    private void AddCard() {
        EffectManager effectManager = new EffectManager(eventBus);
        Card card = new CreatureCard(cardData);
        handPresenter.Hand.Add(card);
    }
}
