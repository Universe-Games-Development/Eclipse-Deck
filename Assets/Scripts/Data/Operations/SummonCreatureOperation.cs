using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Linq;
using UnityEngine;

[OperationFor(typeof(SummonOperationData))]
public class SummonCreatureOperation : GameOperation {
    private const string SpawnZoneKey = "spawnZone";
    private readonly SummonOperationData _data;

    private readonly ICreatureFactory<Card3DView> creatureFactory;

    public SummonCreatureOperation(SummonOperationData data, ICreatureFactory<Card3DView> creatureFactory) {
        _data = data;
        this.creatureFactory = creatureFactory;

        ZoneRequirement allyZone = TargetRequirements.AllyZone;
        AddTarget(SpawnZoneKey, allyZone);
    }

    public override bool Execute() {
        if (!TryGetTypedTarget(SpawnZoneKey, out Zone zone)) {
            Debug.LogError($"Valid {SpawnZoneKey} not found");
            return false;
        }

        if (Source is not CreatureCard creatureCard) {
            Debug.LogError($"{this}: Creature card is null");
            return false;
        }

        // 1. Створюємо істоту
        var creature = creatureFactory.CreateModel(creatureCard);
        if (creature == null) return false;

        // 4. Створюємо візуальну задачу
        var summonTask = visualTaskFactory.Create<SummonVisualTask>(_data.visualTemplate);
        // 2. Створюємо runtime контекст із template
        summonTask.SetRuntimeData(
            creature,
            creatureCard,
            zone
        );
        visualManager.Push(summonTask);

        // 5. Виконуємо логіку
        zone.PlaceCreature(creature);

        return true;
    }
}

public class SummonVisualTask : VisualTask {
    private readonly ICreatureFactory<Card3DView> _creatureFactory;
    private readonly ICardFactory<Card3DView> _cardFactory;

    private SummonVisualData _data;
    public Creature Creature { get; private set; }
    public CreatureCard CreatureCard { get; private set; }
    public Zone Zone { get; private set; }

    public SummonVisualTask(SummonVisualData context, ICreatureFactory<Card3DView> creatureFactory, ICardFactory<Card3DView> cardFactory) {
        _data = context;
        _creatureFactory = creatureFactory;
        _cardFactory = cardFactory;
    }

    public void SetRuntimeData(Creature creature, CreatureCard creatureCard, Zone zone) {
        Creature = creature;
        CreatureCard = creatureCard;
        Zone = zone;
    }

    public override async UniTask Execute() {
        if (CreatureCard == null || Zone == null) {
            Debug.Log($"runtime data is not set for : {this}");
        }
        // 1. Отримуємо презентери
        CardPresenter cardPresenter = UnitRegistry.GetPresenter<CardPresenter>(CreatureCard);
        ZonePresenter zonePresenter = UnitRegistry.GetPresenter<ZonePresenter>(Zone);

        System.Collections.Generic.List<UnitPresenter> unitPresenters = UnitRegistry.GetAllPresenters().Where(presenter => presenter is CardPresenter).ToList();
        if (cardPresenter == null) {
            Debug.LogWarning($"Missing Card presenter for : {CreatureCard}");
            return;
        }
        // 2. Ефект трансформації карти
        Vector3 spawnPosition = cardPresenter.transform.position;

        // 3.Видаляємо карту
        _cardFactory.RemovePresenter(cardPresenter);

        // 4. Створюємо істоту
        CreaturePresenter creaturePresenter = _creatureFactory.SpawnPresenter(Creature);

        Debug.Log($"CREATURE Created : {creaturePresenter} for {Creature}");
        creaturePresenter.transform.position = spawnPosition;

        // 5. Анімація переміщення
        Vector3 zonePosition = zonePresenter.transform.position + _data.aligmentHeightOffset;
        Tweener aligmentAnimation = creaturePresenter.transform.DOMove(zonePosition, _data.aboveAligmentDuration).SetEase(_data.moveEase);
        await creaturePresenter.View.DoTweener(aligmentAnimation);

        // 5. Анімація переміщення
        await PlayMaterializationEffect(creaturePresenter);
    }

    private async UniTask PlayMaterializationEffect(CreaturePresenter creaturePresenter) {
        GameObject materializationEffect = _data.materializationEffect;
        if (materializationEffect != null) {
            var effect = Object.Instantiate(materializationEffect, creaturePresenter.transform.position, Quaternion.identity);
            Object.Destroy(effect, 2f);
        }

        await UniTask.Delay((int)(_data.materializationDelay * 1000));
    }
}

