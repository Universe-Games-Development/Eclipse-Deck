using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class ZonePresenter : UnitPresenter, IDisposable {
    public Zone Zone;
    public ZoneView ZoneView;

    [Inject] IUnitRegistry _unitRegistry;
    [Inject] private IVisualTaskFactory _visualTaskFactory;
    [Inject] IVisualManager _visualManager;
    [Inject] IUnitSpawner<Creature, CreatureView, CreaturePresenter> _creatureSpawner;

    public readonly Dictionary<Creature, CreaturePresenter> creaturesInZone = new();

    public ZonePresenter(Zone zone, ZoneView zone3DView) : base (zone, zone3DView){
        Zone = zone;
        ZoneView = zone3DView;
    }

    private void Start() {
        Zone.OnCreaturePlaced += HandleCreaturePlacement;
        Zone.OnCreatureRemoved += HandleCreatureRemove;
    }

    private void HandleCreaturePlacement(Creature creature) {

        // 1. Створюємо завдання для додавання істоти
        var addTask = new AddCreatureToZoneVisualTask(creature, this, _unitRegistry);

        // 2. Створюємо завдання для реорганізації
        var rearrangeTask = new RearrangeZoneVisualTask(this, cardsOrganizeDuration);

        // 3. Додаємо обидва завдання в чергу
        _visualManager.Push(addTask);
        _visualManager.Push(rearrangeTask);

        ZoneView.UpdateSummonedCount(Zone.Creatures.Count);
    }

    private void HandleCreatureRemove(Creature creature) {
        if (creaturesInZone.TryGetValue(creature, out CreaturePresenter creaturePresenter)) {
            creaturesInZone.Remove(creature);
            _creatureSpawner.RemoveUnit(creaturePresenter);
        }


        var rearrangeTask = new RearrangeZoneVisualTask(this, cardsOrganizeDuration);
        _visualManager.Push(rearrangeTask);

        ZoneView.UpdateSummonedCount(Zone.Creatures.Count);
    }

    public bool PlaceCreature(Creature creature) {
        Zone.TryPlaceCreature(creature);
        return true;
    }

    [SerializeField] private float cardsOrganizeDuration = 0.5f;
    // VISUAL TASK
    public async UniTask RearrangeCreatures(List<LayoutPoint> positions, float duration) {
        var tasks = new List<UniTask>();
        List<CreaturePresenter> creatures = creaturesInZone.Values.ToList();

        for (int i = 0; i < creatures.Count; i++) {
            if (i < positions.Count) {
                CreatureView view = creatures[i].CreatureView;
                LayoutPoint point = positions[i];

                Tweener moveTween = ZoneView.transform.DOMove(point.position, duration)
                                    .SetEase(Ease.OutQuad)
                                    .SetLink(view.gameObject);

                tasks.Add(view.DoTweener(moveTween));
            }
        }
        await UniTask.WhenAll(tasks);
    }


    public void Dispose() {
    }
}

public class RearrangeZoneVisualTask : VisualTask {
    private readonly ZonePresenter _zoneView;
    private readonly float _duration;

    public RearrangeZoneVisualTask(ZonePresenter zonePresenter, float duration = 0.5f) {
        _zoneView = zonePresenter;
        _duration = duration;
    }

    public override async UniTask<bool> Execute() {
        // Отримуємо актуальні позиції від View
        var positions = _zoneView.ZoneView.GetCreaturePoints(_zoneView.Zone.Creatures.Count);

        // Анімуємо переміщення всіх істот
        await _zoneView.RearrangeCreatures(positions, _duration);
        return true;
    }
}


public class AddCreatureToZoneVisualTask : VisualTask {
    private readonly Creature _creature;
    private readonly ZonePresenter _zonePresenter;
    private readonly IUnitRegistry _unitRegistry;

    public AddCreatureToZoneVisualTask(
        Creature creature,
        ZonePresenter zoneView,
        IUnitRegistry viewRegistry) {
        _creature = creature;
        _zonePresenter = zoneView;
        _unitRegistry = viewRegistry;
    }

    public override async UniTask<bool> Execute() {
        // 1. Знаходимо презентер для істоти
        CreaturePresenter creaturePresenter = _unitRegistry.GetPresenter<CreaturePresenter>(_creature);

        if (creaturePresenter == null) {
            Debug.LogWarning($"Failed to find view for creature: {_creature}");
            return false;
        }

        // 2. Додаємо до списку істот в зоні
        _zonePresenter.creaturesInZone.Add(_creature, creaturePresenter);

        // 3. Можна додати ефект появи (опціонально)
        await PlaySpawnEffect(creaturePresenter);
        return true;
    }

    private async UniTask PlaySpawnEffect(CreaturePresenter creaturePresenter) {
        // Ефект появи істоти (спалах, анімація)
        //await view.View.PlaySpawnAnimation();
        await UniTask.Delay(10);
    }
}

public interface ISizeProvider {
    Vector3 GetSize();
    event Action<Vector3> OnSizeChanged;
}