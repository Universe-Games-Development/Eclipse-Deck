using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "SummonVisualData", menuName = "Operations/Visuals/SummonVisualData")]
public class SummonVisualData : VisualData {
    [Header("Card Transform Effect")]
    public GameObject transformEffectPrefab; // Блиск, іскри, дим
    public float transformDuration = 5f;
    public AnimationCurve transformCurve;

    [Header("Creature Materialization")]
    public GameObject materializationEffect; // Портал, світло
    public float materializationDelay;

    // Runtime дані
    public Creature _creature;
    public CreatureCard _creatureCard;
    public Zone _zone;

    public void SetSummonData(Creature creature, CreatureCard creatureCard, Zone zone) {
        _creature = creature;
        _creatureCard = creatureCard;
        _zone = zone;
    }
}

public class SummonVisualTask : VisualTask {
    private readonly ICreatureFactory<Card3DView> _creatureFactory;
    private readonly ICardFactory<Card3DView> _cardFactory;

    private SummonVisualData _data;

    public SummonVisualTask(SummonVisualData data, ICreatureFactory<Card3DView> creatureFactory, ICardFactory<Card3DView> cardFactory) {
        _data = data;
        _creatureFactory = creatureFactory;
        _cardFactory = cardFactory;
    }

    public override async UniTask Execute() {
        
        CardPresenter cardPresenter = UnitRegistry.GetPresenter<CardPresenter>(_data._creatureCard);
        ZonePresenter zonePresenter = UnitRegistry.GetPresenter<ZonePresenter>(_data._zone);
        Vector3 spawnPosition = cardPresenter.transform.position;
        _cardFactory.RemovePresenter(cardPresenter);
        

        Vector3 zonePosition = zonePresenter.transform.position + Vector3.up * 3f;

        

        CreaturePresenter creaturePresenter = _creatureFactory.SpawnPresenter(_data._creature);
        creaturePresenter.transform.position = spawnPosition;

        Tweener aligmentAnimation = creaturePresenter.transform.DOMove(zonePosition, 0.5f);
        await creaturePresenter.View.DoTweener(aligmentAnimation);
        

        // Animation Simulation
        await UniTask.WaitForSeconds(_data.transformDuration);
    }
}

public interface IVisualTaskFactory {
    TVisualTask Create<TVisualTask>(VisualData data) where TVisualTask : VisualTask;
}

public class VisualTaskFactory : IVisualTaskFactory {
    [Inject] DiContainer container;
    public TVisualTask Create<TVisualTask>(VisualData data) where TVisualTask : VisualTask {
        return container.Instantiate<TVisualTask>(new object[] { data });
    }
}

[OperationFor(typeof(SummonOperationData))]
public class SummonCreatureOperation : GameOperation {
    private const string SpawnZoneKey = "spawnZone";
    private readonly SummonOperationData _data;

    private readonly ICreatureFactory<Card3DView> _creatureFactory;

    public SummonCreatureOperation(SummonOperationData data, ICreatureFactory<Card3DView> spawnService) {
        _data = data;
        _creatureFactory = spawnService;

        ZoneRequirement allyZone = TargetRequirements.AllyZone;
        RequestTargets.Add(new Target(SpawnZoneKey, allyZone));
    }

    public override bool Execute() {
        if (!TryGetTarget(SpawnZoneKey, out Zone zone)) {
            Debug.LogError($"Valid {SpawnZoneKey} not found");
            return false;
        }

        CreatureCard creatureCard = Source as CreatureCard;
        if (creatureCard == null) {
            Debug.LogError($"{this}: Creature card is null");
            return false;
        }

        Creature creature = _creatureFactory.CreateModel(creatureCard);
        if (creature == null) return false;

        _data.visualData.SetSummonData(creature, creatureCard, zone);

        SummonVisualTask summonVisualTask = VisualTaskFactory.Create<SummonVisualTask>(_data.visualData);
        VisualManager.Push(summonVisualTask);

        zone.PlaceCreature(creature);

        return true;
    }
}

