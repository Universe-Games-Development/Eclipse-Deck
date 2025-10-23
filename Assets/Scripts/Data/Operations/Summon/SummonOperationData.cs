using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "Summon", menuName = "Operations/Summon")]
public class SummonOperationData : OperationData {
    [SerializeField] public SacrificeOperationData SacrificeOperationData;

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
    [Inject] private readonly IUnitRegistry _unitRegistry;
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
        bool isSummoned = _zone.TrySummonCreature(creature);
        if (isSummoned) {
            StartVisualTask(creature);
        }

        // 4. Логічно додаємо істоту в зону (синхронно)
        return isSummoned;
    }



    private void StartVisualTask(Creature creature) {
        if (!_unitRegistry.TryGetPresenterByModel<CardPresenter>(_creatureCard, out var cardPresenter)) {
            Debug.LogWarning($"[SummoningVisualTask] Card presenter not found for {_creatureCard.UnitName}");
            return;
        }

        if (!_unitRegistry.TryGetPresenterByModel<ZonePresenter>(_zone, out var zonePresenter)) {
            Debug.LogWarning($"[SummoningVisualTask] Zone presenter not found for {_zone.UnitName}");
            return;
        }

        SummoningVisualTask summoningVisualTask = visualTaskFactory.Create<SummoningVisualTask>(
           _data.visualData,
           creature,
           cardPresenter,
           zonePresenter
           );

        // 3. Додаємо в чергу візуальних завдань
        _visualManager.Push(summoningVisualTask);
    }
}

//Summon Effects will depends on card rarity
public class SummoningVisualTask : VisualTask {
    private readonly SummonVisualData _visualData;
   
    private readonly Creature _creature;

    private readonly CardPresenter _cardPresenter;
    private readonly ZonePresenter _zonePresenter;

    private readonly IUnitSpawner<Creature, CreatureView, CreaturePresenter> _creatureSpawner;

    private readonly Vector3 _spawnPosition;

    public SummoningVisualTask(
       SummonVisualData visualData,
       Creature creature,
       CardPresenter cardPresenter,
       ZonePresenter zonePresenter,
       IUnitSpawner<Creature, CreatureView, CreaturePresenter> creatureSpawner) {
        _visualData = visualData;
        _creature = creature;

        _cardPresenter = cardPresenter;
        _zonePresenter = zonePresenter;
        _creatureSpawner = creatureSpawner;

        _spawnPosition = cardPresenter.CardView.transform.position;
    }
    public override async UniTask<bool> ExecuteAsync() {
        _cardPresenter.CardView.gameObject.SetActive(false);

        // Spawn creature view
        var creaturePresenter = _creatureSpawner.SpawnUnit(_creature);
        var creatureView = creaturePresenter.CreatureView;

        // Set initial position
        creatureView.transform.position = _spawnPosition;

        // Animate summon
        await AnimateSummonAsync(creatureView);

        // Add to zone layout
        _zonePresenter.AddCreatureVisual(creaturePresenter);

        return true;
    }

    private async UniTask AnimateSummonAsync(CreatureView creatureView) {
        var duration = _visualData.transformDuration * TimeModifier;
        var fadeOutDuration = duration * 0.5f;

        var sequence = DOTween.Sequence()
            .Append(AnimateShake(creatureView.transform, fadeOutDuration))
            .Join(AnimateScale(creatureView.transform, fadeOutDuration));

        await sequence.Play().ToUniTask();
    }

    private Tween AnimateShake(Transform transform, float duration) {
        const float shakeStrength = 0.5f;
        const int vibrato = 10;

        return transform.DOShakePosition(
            duration,
            strength: shakeStrength,
            vibrato: vibrato,
            randomness: 90,
            fadeOut: true
        );
    }

    private Tween AnimateScale(Transform transform, float duration) {
        transform.localScale = Vector3.zero;
        return transform.DOScale(Vector3.one, duration).SetEase(Ease.OutBack);
    }
}


