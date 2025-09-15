using UnityEngine;
using Zenject;

public interface ICreatureFactory {
    Creature SpawnCreature(CreatureCard card, Vector3? spawnPosition = null);
}

public class CreatureFactory : MonoBehaviour, ICreatureFactory {
    [SerializeField] private Card3DPool creaturePool;

    [Inject] private DiContainer container;
    [Inject] private IUnitPresenterRegistry unitRegistry;

    public Creature SpawnCreature(CreatureCard card, Vector3? spawnPosition = null) {
        if (card == null) {
            Debug.LogError("[CreatureFactory] Cannot spawn creature: CreatureCard is null");
            return null;
        }

        try {
            // Створюємо модель
            var creatureModel = new Creature(card);

            // Визначаємо позицію спавну
            var finalSpawnPosition = DetermineSpawnPosition(card, spawnPosition);

            // Створюємо presenter
            var creaturePresenter = CreateCreaturePresenter(creatureModel, finalSpawnPosition);
            if (creaturePresenter == null) {
                Debug.LogError($"[CreatureFactory] Failed to create presenter for creature: {card}");
                return null;
            }

            // Реєструємо один раз
            unitRegistry.Register(creatureModel, creaturePresenter);

            Debug.Log($"[CreatureFactory] Successfully spawned creature: {card}");
            return creatureModel;
        } catch (System.Exception ex) {
            Debug.LogException(ex);
            Debug.LogError($"[CreatureFactory] Exception while spawning creature {card}: {ex.Message}");
            return null;
        }
    }

    public void DestroyCreature(Creature creature) {
        if (creature == null) return;

        try {
            CreaturePresenter cardPresenter = unitRegistry.GetPresenter<CreaturePresenter>(creature);
            // Отримуємо presenter перед видаленням з реєстру
            if (cardPresenter) {
                // Видаляємо з реєстру
                unitRegistry.Unregister(creature);

                // Повертаємо view в пул
                if (cardPresenter.View != null) {
                    creaturePool.Release(cardPresenter.View);
                }

                // Очищуємо presenter
                if (cardPresenter != null) {
                    cardPresenter.Reset();
                }
            }

            Debug.Log($"[CreatureFactory] Destroyed creature: {creature.Data.Name}");
        } catch (System.Exception ex) {
            Debug.LogException(ex);
            Debug.LogError($"[CreatureFactory] Exception while destroying creature {creature.Data.Name}: {ex.Message}");
        }
    }

    public bool CanSpawnCreature(CreatureCard creatureCard) {
        return creatureCard != null &&
               creaturePool != null &&
               container != null &&
               unitRegistry != null;
    }

    private Vector3 DetermineSpawnPosition(CreatureCard card, Vector3? overridePosition) {
        // Якщо передана позиція - використовуємо її
        if (overridePosition.HasValue) {
            return overridePosition.Value;
        }

        // Спробуємо знайти presenter карти
        CardPresenter cardPresenter = unitRegistry.GetPresenter<CardPresenter>(card);
        if (cardPresenter) {
            return cardPresenter.transform.position;
        }

        // Fallback до Vector3.zero
        return Vector3.zero;
    }

    private CreaturePresenter CreateCreaturePresenter(Creature creature, Vector3 spawnPosition) {
        if (creaturePool == null) {
            Debug.LogError("[CreatureFactory] CreaturePool is not assigned");
            return null;
        }

        try {
            // Отримуємо view з пулу
            var view = creaturePool.Get();
            if (view == null) {
                Debug.LogError("[CreatureFactory] Failed to get CreatureView from pool");
                return null;
            }

            // Налаштовуємо view
            view.name = $"Creature_{creature.Data.Name}_{creature.GetHashCode()}";
            view.transform.position = spawnPosition;

            // Створюємо або отримуємо presenter
            CreaturePresenter presenter;
            if (!view.TryGetComponent(out presenter)) {
                //presenter = container.InstantiateComponent<CreaturePresenter>(view.gameObject);
                presenter = view.gameObject.AddComponent<CreaturePresenter>();
                if (presenter == null) {
                    Debug.LogError($"[CreatureFactory] Failed to create CreaturePresenter component");
                    creaturePool.Release(view); // Повертаємо view назад в пул
                    return null;
                }
            }

            // Ініціалізуємо presenter
            presenter.Initialize(creature, view);

            return presenter;
        } catch (System.Exception ex) {
            Debug.LogException(ex);
            Debug.LogError($"[CreatureFactory] Exception while creating presenter: {ex.Message}");
            return null;
        }
    }
}