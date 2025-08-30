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

    [SerializeField] CreatureCardData creatureCardData;
    [SerializeField] SpellCardData spellCardData;

    [Inject] GameEventBus eventBus;

    private CardFactory cardFactory;
    [Inject] DiContainer diContainer;

    private void Start() {
        cardFactory = new CardFactory(diContainer);

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
        float value = UnityEngine.Random.Range(0f, 1f);
        CardData dataToChoose = creatureCardData;
        if (value > 0.5f) dataToChoose = spellCardData;

        EffectManager effectManager = new EffectManager(eventBus);
        Card card = cardFactory.CreateCard(dataToChoose);
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
