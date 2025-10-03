using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class AreaPresenter : InteractablePresenter {
    public Action<Vector3> OnSizeChanged;
    public AreaView AreaView;
    public AreaPresenter(UnitModel model, AreaView view) : base(model, view) {
        AreaView = view;
    }

    public Vector3 CurrentSize { get; protected set; }

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

    
    private List<CreaturePresenter> creatures = new();
    

    public ZonePresenter(Zone zone, ZoneView zoneView) : base(zone, zoneView) {
        Zone = zone;
        ZoneView = zoneView;

        if (zoneView.settings == null) {
            Debug.LogWarning("Settings layout null for " + this);
            return;
        }


        Zone.OnCreaturePlaced += HandleCreaturePlacement;
        Zone.OnCreatureRemoved += HandleCreatureRemove;

        ZoneView.OnRemoveDebugRequest += TryRemoveCreatureDebug;
        ZoneView.OnUpdateRequest += UpdateVisuals;

        ZoneView.ChangeColor(Zone.OwnerId != null ? Color.green : Color.black);

        UpdateSize();
    }

    private void TryRemoveCreatureDebug() {
        Creature creature = Zone.Creatures.FirstOrDefault();
        Zone.RemoveCreature(creature);
    }

    private void HandleCreaturePlacement(Creature creature) {
        if (!_unitRegistry.TryGetPresenterByModel<CreaturePresenter>(creature, out var presenter)) {
            Debug.LogWarning("Failed to find presenter for : " + creature);
        }

        creatures.Add(presenter);
       

        var addTask = new AddCreatureToZoneVisualTask(creature, this, _unitRegistry);
        _visualManager.Push(addTask);
        RearrangeCreatures();
    }

    private void HandleCreatureRemove(Creature creature) {
        if (!_unitRegistry.TryGetPresenterByModel<CreaturePresenter>(creature, out var presenter)) {
            Debug.LogWarning("Failed to find presenter for : " + creature);
        }

        if (presenter != null) {
            creatures.Remove(presenter);
            ZoneView.RemoveCreatureView(presenter.CreatureView);
            _unitRegistry.Unregister(presenter);
        }
        RearrangeCreatures();
    }

    private void UpdateVisuals() {
        RearrangeCreatures();
        UpdateSize();
    }

    public void RearrangeCreatures() {
        var rearrangeTask = new RearrangeZoneVisualTask(ZoneView, 0.1f);
        _visualManager.Push(rearrangeTask);
    }

    private void UpdateSize() {
        Vector3 size = ZoneView.CalculateSize(Zone.MaxCreatures);

        ChangeSize(size);
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