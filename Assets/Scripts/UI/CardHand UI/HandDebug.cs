using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class HandDebug : MonoBehaviour
{
    [SerializeField] HandPresenter handPresenter;

    [SerializeField] Button addCardButton;
    [SerializeField] Button removeCardButton;

    [SerializeField] int initialCards = 0;
    [SerializeField] TextMeshProUGUI hoveredCard;

    [Inject] IEventBus<IEvent> eventBus;

    [Inject] private ICardFactory cardFactory;
    [Inject] CardProvider cardProvider;
    [SerializeField] List<CardData> cardDatas;

    private void Start() {

        addCardButton?.onClick.AddListener(() => {
            AddCard();
        });

        removeCardButton?.onClick.AddListener(() => {
            RemoveRandomCard();
        });

        handPresenter.OnCardHovered += HandleHover;

        for (int i = 0; i < initialCards; i++) {
            AddCard();
        }
    }

    private void RemoveRandomCard() {
        List<Card> cards = handPresenter.GetCards();
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

    private void HandleHover(CardPresenter presenter, bool isHovered) {
        if (hoveredCard == null) return;

        if (isHovered) {
            hoveredCard.text = presenter.Card.Data.Name;
        } else {
            hoveredCard.text = "No Card";
        }
    }
}


public class GameController {

    public CreaturePresenter SpawnCreature() {
        return null;
    } 
}