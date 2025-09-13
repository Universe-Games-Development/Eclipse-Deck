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
        // ��������� ������ ������ � �����
        var creature = CreateCreature(card);

        var creaturePresenter = CreateCreaturePresenter(creature);

        return creaturePresenter;
    }

    public CreaturePresenter CreateCreaturePresenter(Creature creature) {
        // ��������� View
        var view = creaturePool.Get();

        // ��������� GameObject ��� Presenter
        view.name = $"Creature_{creature.Data.Name}";

        // ������ Presenter ����� DI
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