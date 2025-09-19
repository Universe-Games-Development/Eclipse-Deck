using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class ZonePresenter : UnitPresenter {
    public Zone Zone;
    public Zone3DView View;
    [SerializeField] public BoardPlayerPresenter Owner;
    [Inject] IUnitPresenterRegistry _presenterRegistry;
    [Inject] private IVisualTaskFactory _visualTaskFactory;
    [Inject] IVisualManager _visualManager;
    [Inject] ICreatureFactory<Card3DView> _creatureFactory;

    private readonly List<CreaturePresenter> creaturesInZone = new();
    public int CreaturesCount => creaturesInZone.Count;

    private void Awake() {
        Zone = new Zone();
    }

    private void Start() {
        Zone.ChangeOwner(Owner.Opponent);
        Zone.OnCreaturePlaced += HandleCreaturePlacement;
        Zone.OnCreatureRemoved += HandleCreatureRemove;
        RegisterInGame();

        if (doUpdate) {
            _ = DoTestUpdate();
        }
    }

    public void Initialize(Zone3DView zone3DView, Zone zone) {
        View = zone3DView;
        Zone = zone;
    }

    [SerializeField] float updateTimer = 1f;
    [SerializeField] bool doUpdate = false;

    private async UniTask DoTestUpdate() {
        while (doUpdate) {
            var positions = View.GetCreaturePoints(creaturesInZone.Count);
            await RearrangeCreatures(positions, updateTimer * 1000);
        }
    }

    public void AddCreaturePresenter(CreaturePresenter presenter) {
        if (!creaturesInZone.Contains(presenter)) {
            creaturesInZone.Add(presenter);
        }
    }

    private void HandleCreaturePlacement(Creature creature) {
        DebugLog($"{creature} placed in zone");

        // 1. Створюємо завдання для додавання істоти
        var addTask = new AddCreatureToZoneVisualTask(creature, this, _presenterRegistry);

        // 2. Створюємо завдання для реорганізації
        var rearrangeTask = new RearrangeZoneVisualTask(this, cardsOrganizeDuration);

        // 3. Додаємо обидва завдання в чергу
        _visualManager.Push(addTask);
        _visualManager.Push(rearrangeTask);

        View.UpdateSummonedCount(Zone.GetCreaturesCount());
    }

    private void HandleCreatureRemove(Creature creature) {
        DebugLog("Creature removed from zone");

        // Знаходимо і видаляємо презентер
        var creatureToRemove = creaturesInZone.FirstOrDefault(c => c.Creature == creature);
        if (creatureToRemove != null) {
            creaturesInZone.Remove(creatureToRemove);

            // Створюємо завдання для реорганізації після видалення


            _creatureFactory.DestroyCreature(creatureToRemove);
        }

        var rearrangeTask = new RearrangeZoneVisualTask(this, cardsOrganizeDuration);
        _visualManager.Push(rearrangeTask);

        View.UpdateSummonedCount(Zone.GetCreaturesCount());
    }

    public bool PlaceCreature(Creature creature) {
        DebugLog($"Placing creature: {creature}");
        Zone.PlaceCreature(creature);
        return true;
    }

    [SerializeField] private float cardsOrganizeDuration = 0.5f;
    // VISUAL TASK
    public async UniTask RearrangeCreatures(List<LayoutPoint> positions, float duration) {
        var tasks = new List<UniTask>();
        for (int i = 0; i < creaturesInZone.Count; i++) {
            if (i < positions.Count) {
                CreaturePresenter creaturePresenter = creaturesInZone[i];
                LayoutPoint point = positions[i];

                Tweener moveTween = creaturePresenter.transform.DOMove(point.position, duration)
                                    .SetEase(Ease.OutQuad)
                                    .SetLink(creaturePresenter.gameObject);

                tasks.Add(creaturePresenter.View.DoTweener(moveTween));
            }
        }
        await UniTask.WhenAll(tasks);
    }

    public override void Highlight(bool enable) {
        View.Highlight(enable);
    }

    #region UnitInfo API
    public override UnitModel GetModel() {
        return Zone;
    }

    #endregion
}

public class RearrangeZoneVisualTask : VisualTask {
    private readonly ZonePresenter _zonePresenter;
    private readonly float _duration;

    public RearrangeZoneVisualTask(ZonePresenter zonePresenter, float duration = 0.5f) {
        _zonePresenter = zonePresenter;
        _duration = duration;
    }

    public override async UniTask Execute() {
        // Отримуємо актуальні позиції від View
        var positions = _zonePresenter.View.GetCreaturePoints(_zonePresenter.CreaturesCount);

        // Анімуємо переміщення всіх істот
        await _zonePresenter.RearrangeCreatures(positions, _duration);
    }
}

public class CreaturesArrangeVisualData : VisualData {
    public List<CreaturePresenter> Creatures { get; internal set; }
    public List<LayoutPoint> Positions { get; internal set; }
    public float Duration { get; internal set; }
}


public class AddCreatureToZoneVisualTask : VisualTask {
    private readonly Creature _creature;
    private readonly ZonePresenter _zonePresenter;
    private readonly IUnitPresenterRegistry _presenterRegistry;

    public AddCreatureToZoneVisualTask(
        Creature creature,
        ZonePresenter zonePresenter,
        IUnitPresenterRegistry presenterRegistry) {
        _creature = creature;
        _zonePresenter = zonePresenter;
        _presenterRegistry = presenterRegistry;
    }

    public override async UniTask Execute() {
        // 1. Знаходимо презентер для істоти
        CreaturePresenter creaturePresenter = _presenterRegistry.GetPresenter<CreaturePresenter>(_creature);

        if (creaturePresenter == null) {
            Debug.LogWarning($"Failed to find presenter for creature: {_creature}");
            return;
        }

        // 2. Додаємо до списку істот в зоні
        _zonePresenter.AddCreaturePresenter(creaturePresenter);

        // 3. Можна додати ефект появи (опціонально)
        await PlaySpawnEffect(creaturePresenter);
    }

    private async UniTask PlaySpawnEffect(CreaturePresenter presenter) {
        // Ефект появи істоти (спалах, анімація)
        //await presenter.View.PlaySpawnAnimation();
        await UniTask.Delay(10);
    }
}