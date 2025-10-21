using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "Summon", menuName = "Operations/Summon")]
public class SummonOperationData : OperationData {
    [SerializeField] public SacrificeOperationData sacrificeOperationData;

    public override GameOperation CreateOperation(IOperationFactory factory, TargetRegistry targetRegistry) {
        Zone zone = targetRegistry.Get<Zone>(TargetKeys.MainTarget);
        CreatureCard creatureCard = targetRegistry.Get<CreatureCard>(TargetKeys.SourceCard);

        return factory.Create<SummonCreatureOperation>(this, creatureCard, zone);
    }

    protected override void BuildDefaultRequirements() {
        AddRequirement(RequirementPresets.AllyZone(TargetKeys.MainTarget));
    }
}

public class SummonCreatureOperation : GameOperation {
    private readonly SummonOperationData _data;
    private readonly Zone _zone;
    private readonly CreatureCard _creatureCard;

    [Inject] private readonly IEntityFactory _entityFactory;
    [Inject] private readonly IVisualManager _visualManager;
    [Inject] private readonly IVisualTaskFactory visualTaskFactory;

    public SummonCreatureOperation(SummonOperationData summonData, Zone zone, CreatureCard creatureCard) {
        _data = summonData;
        _zone = zone;
        _creatureCard = creatureCard;
    }

    public override bool Execute() {
        if (_zone.IsFull()) {
            return false;
        }

        // 1. Створюємо модель істоти
        Creature creature = _entityFactory.CreateCreatureFromCard(_creatureCard);

        SummoningVisualTask summoningVisualTask = visualTaskFactory.Create<SummoningVisualTask>(
            creature,
            _creatureCard,
            _zone
            );

        // 3. Додаємо в чергу візуальних завдань
        _visualManager.Push(summoningVisualTask);

        // 4. Логічно додаємо істоту в зону (синхронно)
        return _zone.TrySummonCreature(creature);
    }
}
public class SummoningVisualTask : VisualTask {
    private readonly Creature _creature;
    private readonly CreatureCard _creatureCard;
    private readonly Zone _zone;
    private readonly IEntityFactory _entityFactory;
    private readonly IUnitRegistry _unitRegistry;
    private readonly IUnitSpawner<Creature, CreatureView, CreaturePresenter> _creatureSpawner;

    public SummoningVisualTask(
        Creature creature,
        CreatureCard creatureCard,
        Zone zone,
        IEntityFactory entityFactory,
        IUnitRegistry unitRegistry,
        IUnitSpawner<Creature, CreatureView, CreaturePresenter> creatureSpawner) {
        _creature = creature;
        _creatureCard = creatureCard;
        _zone = zone;
        _entityFactory = entityFactory;
        _unitRegistry = unitRegistry;
        _creatureSpawner = creatureSpawner;
    }

    public override async UniTask<bool> ExecuteAsync() {
        // 1. Знаходимо CardView для анімації "з картки"
        if (!_unitRegistry.TryGetPresenterByModel<CardPresenter>(_creatureCard, out var cardPresenter)) {
            Debug.LogWarning($"Card presenter not found for {_creatureCard}");
            return await CreateCreatureDirectly();
        }

        Vector3 cardPosition = cardPresenter.CardView.transform.position;

        // 2. Створюємо CreaturePresenter та CreatureView
        CreaturePresenter creaturePresenter = _creatureSpawner.SpawnUnit(_creature);
        CreatureView creatureView = creaturePresenter.CreatureView;

        // 3. Налаштовуємо початкову позицію (там де картка)
        creatureView.transform.position = cardPosition;
        creatureView.gameObject.SetActive(true);

        // 4. Знаходимо ZonePresenter для фінальної позиції
        if (!_unitRegistry.TryGetPresenterByModel<ZonePresenter>(_zone, out var zonePresenter)) {
            Debug.LogWarning($"Zone presenter not found for {_zone}");
            return false;
        }

        // 5. Анімуємо перехід від картки до зони
        await AnimateSummonFromCard(creatureView, cardPosition, zonePresenter);

        // 6. Додаємо в ZonePresenter для подальшого управління
        zonePresenter.AddCreatureVisual(creaturePresenter);

        return true;
    }

    private async UniTask<bool> CreateCreatureDirectly() {
        // Fallback: створюємо без анімації з картки
        CreaturePresenter creaturePresenter = _creatureSpawner.SpawnUnit(_creature);

        if (_unitRegistry.TryGetPresenterByModel<ZonePresenter>(_zone, out var zonePresenter)) {
            zonePresenter.AddCreatureVisual(creaturePresenter);
            return true;
        }

        return false;
    }

    private async UniTask AnimateSummonFromCard(
        CreatureView creatureView,
        Vector3 startPosition,
        ZonePresenter zonePresenter) {
        // Отримуємо фінальну позицію в зоні
        Vector3? targetPosition = zonePresenter.ZoneView.GetCreaturePosition(creatureView);

        if (!targetPosition.HasValue) {
            creatureView.transform.position = zonePresenter.ZoneView.transform.position;
            return;
        }

        // Анімація переміщення
        float duration = 0.8f * TimeModifier;

        Sequence summonSequence = DOTween.Sequence()
            .Join(creatureView.transform.DOMove(targetPosition.Value, duration).SetEase(Ease.OutBack))
            .Join(creatureView.transform.DOScale(Vector3.one, duration * 0.5f).From(Vector3.zero))
            .Join(creatureView.transform.DORotate(Vector3.up * 360f, duration, RotateMode.LocalAxisAdd));

        await summonSequence.Play().ToUniTask();
    }
}