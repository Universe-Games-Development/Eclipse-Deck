using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

public interface ICreatureFactory<TView> {
    Creature CreateModel(CreatureCard card);
    CreaturePresenter SpawnPresenter(Creature creature);
    void DestroyCreature(CreaturePresenter creaturePresenter);
}

public class CreatureFactory<TView> : ICreatureFactory<TView> where TView : CardView {

    [Inject] public IUnitPresenterRegistry _unitRegistry;
    [Inject] private DiContainer container;

    private readonly IComponentPool<TView> _pool;

    public CreatureFactory(IComponentPool<TView> cardPool) {
        _pool = cardPool;
    }

    public Creature CreateModel(CreatureCard card) {
       return new Creature(card);
    }

    public CreaturePresenter SpawnPresenter(Creature creature) {
        if (creature == null) {
            Debug.LogError("[CreatureFactory] Creature is null");
            return null;
        }

        if (_pool == null) {
            Debug.LogError("[CreatureFactory] CreaturePool is not assigned");
            return null;
        }

        // Отримуємо view з пулу
        TView view = _pool.Get();
        if (view == null) {
            Debug.LogError("[CreatureFactory] Failed to get CreatureView from pool");
            return null;
        }

        // Налаштовуємо view
        view.name = $"Creature_{creature.Data.Name}_{creature.GetHashCode()}";

        // Створюємо або отримуємо presenter
        if (!view.TryGetComponent(out CreaturePresenter presenter)) {
            presenter = container.InstantiateComponent<CreaturePresenter>(view.gameObject);
            //presenter = view.gameObject.AddComponent<CreaturePresenter>();
            if (presenter == null) {
                Debug.LogError($"[CreatureFactory] Failed to create CreaturePresenter component");
                _pool.Release(view); // Повертаємо view назад в пул
                return null;
            }
        }

        // Ініціалізуємо presenter
        presenter.Initialize(creature, view);
        _unitRegistry.Register(creature, presenter);

        return presenter;
    }

    public bool CanSpawnCreature(CreatureCard creatureCard) {
        return creatureCard != null &&
               _pool != null &&
               container != null;
    }

    public void DestroyCreature(CreaturePresenter presenter) {
        if (presenter == null) return;

        _unitRegistry.Unregister(presenter);

        if (presenter.View is TView view) {
            _pool.Release(view);
        }
    }
}