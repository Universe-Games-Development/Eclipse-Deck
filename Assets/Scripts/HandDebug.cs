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

    [SerializeField] CreatureCardData creatureCardData;
    [SerializeField] SpellCardData spellCardData;

    [Inject] GameEventBus eventBus;

    private CardFactory cardFactory;
    [Inject] DiContainer diContainer;

    private void Start() {
        cardFactory = new CardFactory(diContainer);

        addCardButton.onClick.AddListener(() => {
            AddCard();
        });

        for (int i = 0; i < initialCards; i++) {
            AddCard();
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
}
