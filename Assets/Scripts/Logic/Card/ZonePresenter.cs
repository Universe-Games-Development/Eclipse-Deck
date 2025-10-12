using System;
using System.Linq;
using UnityEngine;
using Zenject;

public class ZonePresenter : UnitPresenter, IDisposable {
    public Zone Zone;
    public ZoneView ZoneView;
    [Inject] private IUnitRegistry _unitRegistry;

    public ZonePresenter(Zone zone, ZoneView zoneView) : base(zone, zoneView) {
        Zone = zone;
        ZoneView = zoneView;

        Zone.OnChangedOwner += HandleOwnerChanged;
        Zone.OnCreatureSummoned += HandleCreatureSummoned;
        Zone.OnCreatureRemoved += HandleCreatureRemove;
        Zone.ONMaxCreaturesChanged += HandleSizeUpdate;

        ZoneView.OnRemoveDebugRequest += TryRemoveCreatureDebug;

        HandleOwnerChanged(Zone.OwnerId);
        HandleSizeUpdate(Zone.MaxCreatures);

        foreach (var creature in Zone.Creatures) {
            HandleCreatureSummoned(creature);
        }
    }

    private void HandleOwnerChanged(string ownerId) {
        ZoneView.ChangeColor(ownerId != null ? Color.green : Color.black);
    }

    private void HandleSizeUpdate(int maxSize) {
        Vector3 size = ZoneView.CalculateRequiredSize(maxSize);
        ZoneView.Resize(size);
    }

    private void TryRemoveCreatureDebug() {
        Creature creature = Zone.Creatures.FirstOrDefault();
        if (creature != null) {
            Zone.RemoveCreature(creature);
        }
    }

    private void HandleCreatureSummoned(Creature creature) {
        ZoneView.UpdateSummonedCount(Zone.Creatures.Count);
        CreaturePresenter creaturePresenter = _unitRegistry.GetPresenter<CreaturePresenter>(creature);

        ZoneView.SummonCreatureView(creaturePresenter.CreatureView);
    }

    private void HandleCreatureRemove(Creature creature) {
        ZoneView.UpdateSummonedCount(Zone.Creatures.Count);

        if (!_unitRegistry.TryGetPresenterByModel<CreaturePresenter>(creature, out var presenter)) {
            Debug.LogWarning("Failed to find presenter for : " + creature);
            return;
        }

        var view = presenter.CreatureView;
        ZoneView.RemoveCreatureView(view);

        _unitRegistry.Unregister(presenter);
    }

    public void Dispose() {
        Zone.OnChangedOwner -= HandleOwnerChanged;
        ZoneView.OnRemoveDebugRequest -= TryRemoveCreatureDebug;
        Zone.OnCreatureSummoned -= HandleCreatureSummoned;
        Zone.OnCreatureRemoved -= HandleCreatureRemove;
        Zone.ONMaxCreaturesChanged -= HandleSizeUpdate;
    }
}
