using Unity.VisualScripting;
using UnityEngine;
using Zenject;
public interface ICreatureSpawnService {
    CreaturePresenter SpawnCreatureFromCard(CreatureCard card);
}

public class CreatureSpawnService : MonoBehaviour, ICreatureSpawnService {
    [SerializeField] CreaturePool creaturePool;
    [Inject] DiContainer container;
    public CreaturePresenter SpawnCreatureFromCard(CreatureCard card) {
        // Створюємо модель істоти з карти
        var creature = CreateCreature(card);

        var creaturePresenter = CreateCreaturePresenter(creature);

        return creaturePresenter;
    }

    public CreaturePresenter CreateCreaturePresenter(Creature creature) {
        // Створюємо View
        var view = creaturePool.Get();

        // Створюємо GameObject для Presenter
        view.name = $"Creature_{creature.Data.Name}";

        // Додаємо Presenter через DI
        if (!view.TryGetComponent(out CreaturePresenter presenter)) {
            presenter = container.InstantiateComponent<CreaturePresenter>(view.gameObject);
        }

        presenter.Initialize(creature, view);
        return presenter;
    }

    private Creature CreateCreature(CreatureCard card) {
        return new Creature(card);
    }
}