using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class HandDebug : MonoBehaviour {
    [SerializeField] CardHandView handView;

    [SerializeField] Button addCardButton;
    [SerializeField] Button removeCardButton;

    [SerializeField] int initialCards = 0;
    [SerializeField] TextMeshProUGUI hoveredCard;

    [Inject] IEventBus<IEvent> eventBus;

    [Inject] private ICardFactory cardFactory;
    [Inject] CardProvider cardProvider;
    [SerializeField] List<CardData> cardDatas;
    [Inject] IUnitRegistry registry;

    private HandPresenter handPresenter;

    private void Start() {
        if (handView == null) {
            Debug.LogWarning("HandView null");
            return;
        }
        handPresenter = registry.GetPresenter<HandPresenter>(handView);
        if (handPresenter == null) {
            Debug.LogWarning("HandPresenter null");
            return;
        }

        addCardButton?.onClick.AddListener(() => {
            AddCard();
        });

        removeCardButton?.onClick.AddListener(() => {
            RemoveRandomCard();
        });


        for (int i = 0; i < initialCards; i++) {
            AddCard();
        }
    }

    private void RemoveRandomCard() {
        List<Card> cards = handPresenter.GetCards().ToList();
        if (cards.Count() > 0) {
            handPresenter.RemoveCard(cards.Last());
        }
    }

    private void AddCard() {
        List<CardData> cardDatas1 = cardProvider.GetUnlockedCards();
        bool v = cardDatas.TryGetRandomElement(out CardData cardData);

        EffectManager effectManager = new EffectManager(eventBus);
        Card card = cardFactory.CreateCard(cardData);
        handPresenter.AddCard(card);
    }

}
