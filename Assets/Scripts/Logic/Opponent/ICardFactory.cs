using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public interface ICardFactory<TView> {
    Card CreateCard(CardData cardData);
    CardPresenter SpawnPresenter(Card card);
    void RemovePresenter(CardPresenter cardPresenter);
}

public class CardFactory<TView> : ICardFactory<TView> where TView : CardView {
    [Inject] private DiContainer _container;
    [Inject] private IUnitPresenterRegistry _unitRegistry;
    private readonly IComponentPool<TView> _pool;

    public CardFactory(IComponentPool<TView> cardPool) {
        _pool = cardPool;
    }

    public Card CreateCard(CardData cardData) {
        Card card = cardData switch {
            CreatureCardData creatureData => _container.Instantiate<CreatureCard>(new object[] { creatureData }),
            SpellCardData spellData => _container.Instantiate<SpellCard>(new object[] { spellData }),
            _ => throw new ArgumentException($"Unsupported card data type: {cardData.GetType()}")
        };

        return card;
    }

    public CardPresenter SpawnPresenter(Card card) {
        if (card == null) throw new ArgumentNullException(nameof(card));
        if (_pool == null) throw new InvalidOperationException("CardPool is not assigned");

        // Отримуємо view з пулу
        TView view = _pool.Get();
        if (view == null) throw new InvalidOperationException("Failed to get CardView from pool");

        // Налаштовуємо view
        view.name = $"Card {card.Data.Name}_{card.GetHashCode()}";

        // Створюємо або отримуємо presenter
        CardPresenter presenter;
        if (!view.TryGetComponent(out presenter)) {
            presenter = _container.InstantiateComponent<CardPresenter>(view.gameObject);
            if (presenter == null) {
                _pool.Release(view);
                throw new InvalidOperationException("Failed to create CardPresenter component");
            }
        }

        // Ініціалізуємо presenter
        presenter.Initialize(card, view);
        _unitRegistry.Register(card, presenter);

        return presenter;
    }

    public void RemovePresenter(CardPresenter presenter) {
        if (presenter == null) return;

        _unitRegistry.Unregister(presenter);


        if (presenter.View is TView view) {
            _pool.Release(view);
        }
    }
}