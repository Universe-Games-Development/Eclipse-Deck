using Cysharp.Threading.Tasks;
using System;
using System.Linq;
using UnityEngine;
using Zenject;

public class AreaPresenter : InteractablePresenter {
    public Action<Vector3> OnSizeChanged;
    public AreaView AreaView;
    public AreaPresenter(UnitModel model, AreaView view) : base(model, view) {
        AreaView = view;
    }

    public Vector3 CurrentSize { get; private set; }

    public void ChangeSize(Vector3 size) {
        AreaView.SetSize(size);
        CurrentSize = size;
        OnSizeChanged?.Invoke(size);
    }
}

public class ZonePresenter : AreaPresenter, IDisposable {
    public Zone Zone;
    public ZoneView ZoneView;

    [Inject] private IUnitRegistry _unitRegistry;
    [Inject] private IVisualTaskFactory _visualTaskFactory;
    [Inject] private IVisualManager _visualManager;

    public ZonePresenter(Zone zone, ZoneView zoneView) : base(zone, zoneView) {
        Zone = zone;
        ZoneView = zoneView;

        Zone.OnCreaturePlaced += HandleCreaturePlacement;
        Zone.OnCreatureRemoved += HandleCreatureRemove;
        ZoneView.OnRemoveDebugRequest += TryRemoveCreatureDebug;

        InitializeVisuals();
    }

    private void InitializeVisuals() {
        ZoneView.ChangeColor(Zone.OwnerId != null ? Color.green : Color.black);
        Vector3 newSize = ZoneView.CalculateSize(Zone.MaxCreatures);
        ZoneView.SetSize(newSize);
    }

    private void TryRemoveCreatureDebug() {
        Creature creature = Zone.Creatures.FirstOrDefault();
        Zone.RemoveCreature(creature);
    }

    private void HandleCreaturePlacement(Creature creature) {
        var addTask = new AddCreatureToZoneVisualTask(creature, this, _unitRegistry);
        var rearrangeTask = new RearrangeZoneVisualTask(ZoneView);

        _visualManager.Push(addTask);
        _visualManager.Push(rearrangeTask);
    }

    private void HandleCreatureRemove(Creature creature) {
        CreaturePresenter creaturePresenter = _unitRegistry.GetPresenter<CreaturePresenter>(creature);
        if (creaturePresenter != null) {
            ZoneView.RemoveCreatureView(creaturePresenter.CreatureView);
            _unitRegistry.Unregister(creaturePresenter);
        }

        var rearrangeTask = new RearrangeZoneVisualTask(ZoneView);
        _visualManager.Push(rearrangeTask);
    }

    public override void Dispose() {
        base.Dispose();
        ZoneView.OnRemoveDebugRequest -= TryRemoveCreatureDebug;
        Zone.OnCreaturePlaced -= HandleCreaturePlacement;
        Zone.OnCreatureRemoved -= HandleCreatureRemove;
    }
}

public class RearrangeZoneVisualTask : VisualTask {
    private readonly ZoneView _zoneView;
    private readonly float _duration;

    public RearrangeZoneVisualTask(ZoneView zoneView, float duration = 0.5f) {
        _zoneView = zoneView;
        _duration = duration;
    }

    public override async UniTask<bool> Execute() {
        await _zoneView.RearrangeCreatures(_duration);
        return true;
    }
}

public class AddCreatureToZoneVisualTask : VisualTask {
    private readonly Creature _creature;
    private readonly ZonePresenter _zonePresenter;
    private readonly IUnitRegistry _unitRegistry;

    public AddCreatureToZoneVisualTask(Creature creature, ZonePresenter zonePresenter, IUnitRegistry unitRegistry) {
        _creature = creature;
        _zonePresenter = zonePresenter;
        _unitRegistry = unitRegistry;
    }

    public override async UniTask<bool> Execute() {
        CreaturePresenter creaturePresenter = _unitRegistry.GetPresenter<CreaturePresenter>(_creature);
        if (creaturePresenter != null) {
            _zonePresenter.ZoneView.AddCreatureView(creaturePresenter.CreatureView);
            await PlaySpawnEffect(creaturePresenter);
            return true;
        }
        return false;
    }

    private async UniTask PlaySpawnEffect(CreaturePresenter creaturePresenter) {
        // Ефект появи
        await UniTask.WaitForSeconds(0.1f); // Можна замінити на реальну анімацію
    }
}