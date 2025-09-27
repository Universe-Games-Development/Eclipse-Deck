using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

[OperationFor(typeof(SummonOperationData))]
public class SummonCreatureOperation : GameOperation {
    private const string SpawnPlaceKey = "spawnZone";
    private readonly SummonOperationData _data;

    private readonly ICreatureFactory creatureFactory;
    

    public SummonCreatureOperation(SummonOperationData data, ICreatureFactory creatureFactory) {
        _data = data;
        this.creatureFactory = creatureFactory;

        PlaceRequirement allyZone = TargetRequirements.AllyPlace;
        AddTarget(SpawnPlaceKey, allyZone);
    }

    public override bool Execute() {
        if (!TryGetTypedTarget(SpawnPlaceKey, out Zone zone)) {
            Debug.LogError($"Valid {SpawnPlaceKey} not found");
            return false;
        }

        if (Source is not CreatureCard creatureCard) {
            Debug.LogError($"{this}: Creature card is null");
            return false;
        }

        // 1. Створюємо істоту
        var creature = creatureFactory.CreateModel(creatureCard);
        if (creature == null) return false;

        // 2. Створюємо візуальну задачу
        var summonTask = visualTaskFactory.Create<SommonFromCardVisualTask>(
            creature, zone, creatureCard, _data.visualTemplate
        );

        visualManager.Push(summonTask);

        // 3. Виконуємо логіку
        return zone.TryPlaceCreature(creature);
    }
}


// Transform Card into Creature
public class SommonFromCardVisualTask : VisualTask {
    private readonly Creature _creature;
    private readonly SummonVisualData _data;

    [Inject] IUnitSpawner<Creature, CreatureView, CreaturePresenter> _creatureSpawner;
    [Inject] IUnitSpawner<Card, CardView, CardPresenter> _cardSpawner;
    [Inject] IUnitRegistry unitRegistry;

    private CreatureView _creatureView;

    public SommonFromCardVisualTask(Creature creature, SummonVisualData data) {
        _creature = creature;
        _data = data;
    }

    public override async UniTask<bool> Execute() {
        CardPresenter cardPresenter = unitRegistry.GetPresenter<CardPresenter>(_creature.SourceCard);
        Vector3 _spawnPosition = cardPresenter.CardView.transform.position;
        _cardSpawner.RemoveUnit(cardPresenter);

        CreaturePresenter creaturePresenter = _creatureSpawner.SpawnUnit(_creature);
        _creatureView = creaturePresenter.CreatureView;
        _creatureView.transform.position = _spawnPosition;

        // 2. Ефект матеріалізації
        await PlayMaterializationEffect();

        return true;
    }

    private async UniTask PlayMaterializationEffect() {
        if (_data.materializationEffect != null) {
            var effect = GameObject.Instantiate(
                _data.materializationEffect,
                _creatureView.transform.position,
                Quaternion.identity
            );
            GameObject.Destroy(effect, 2f);
        }
        await UniTask.Delay((int)(_data.materializationDelay * 1000));
    }
}

