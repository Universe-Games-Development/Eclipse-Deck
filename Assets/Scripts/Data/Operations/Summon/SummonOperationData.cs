using Cysharp.Threading.Tasks;
using System.Linq;
using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "Summon", menuName = "Operations/Summon")]
public class SummonOperationData : OperationData<SummonCreatureOperation> {
}

public class SummonCreatureOperation : GameOperation {
    private const string SpawnPlaceKey = "spawnZone";
    private readonly SummonOperationData _data;

    [Inject] private readonly IEntityFactory _entityFactory;
    [Inject] private readonly ITargetFiller _targetFiller;
    [Inject] private readonly IOperationFactory _operationFactory;
    [Inject] private readonly IUnitRegistry _unitRegistry;

    [Inject] IUnitSpawner<Creature, CreatureView, CreaturePresenter> _creatureSpawner;

    public SummonCreatureOperation(UnitModel source, SummonOperationData data) : base(source) {
        _data = data;
        AddTarget(new TargetInfo(SpawnPlaceKey, TargetRequirements.AllyPlace));
    }

    public override async UniTask<bool> Execute() {
        if (!TryGetTypedTarget(SpawnPlaceKey, out Zone zone)) {
            return false;
        }

        if (Source is not CreatureCard creatureCard) {
            return false;
        }

        // Перевірка місця і жертвоприношення
        if (zone.IsFull() && !await TrySacrificeCreature(zone)) {
            return false;
        }

        if (!zone.CanSummonCreature()) {
            return false;
        }

        // 1. Створюємо модель істоти
        Creature creature = _entityFactory.CreateCreatureFromCard(creatureCard);

        // 2. ✅ СТВОРЮЄМО PRESENTER ОДРАЗУ (але View неактивне)
        Vector3 spawnPosition = Vector3.zero;
        if (_unitRegistry.TryGetViewByModel(creatureCard, out CardView view)) {
            spawnPosition = view.transform.position;
        }
        var creaturePresenter = _creatureSpawner.SpawnUnit(creature, registerInSystems: true);
        creaturePresenter.CreatureView.gameObject.SetActive(false); // 👈 Приховуємо
        creaturePresenter.CreatureView.transform.position = spawnPosition;

        // 3. Додаємо в зону (логіка)
        if (!zone.TrySummonCreature(creature)) {
            _creatureSpawner.RemoveUnit(creaturePresenter); // Cleanup якщо не вдалося
            return false;
        }

        return true;
    }

    private async UniTask<bool> TrySacrificeCreature(Zone zone) {
        var sacrificeOp = _operationFactory.Create<SacrificeCreatureOperation>(Source, zone);
        var targetInfo = sacrificeOp.GetTargets().First();

        var fillResult = await _targetFiller.TryFillTargetAsync(targetInfo, Source, false);
        if (!fillResult.IsSuccess) {
            return false;
        }

        sacrificeOp.SetTarget(targetInfo.Key, fillResult.Unit);
        return await sacrificeOp.Execute();
    }
}