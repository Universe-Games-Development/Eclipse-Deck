using System;
using System.Linq;
using UnityEngine;
using Zenject;

public class ZonePresenter : UnitPresenter, IDisposable {
    public Zone Zone;
    public ZoneView ZoneView;
    [Inject] private IUnitRegistry _unitRegistry;
    [Inject] IOpponentRegistry opponentRegistry;

    public ZonePresenter(Zone zone, ZoneView zoneView) : base(zone, zoneView) {
        Zone = zone;
        ZoneView = zoneView;

        Zone.OnChangedOwner += HandleOwnerChanged;
        //Zone.OnCreatureSummoned += HandleCreatureSummoned;
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
        if (string.IsNullOrEmpty(ownerId)) {
            ZoneView.ChangeColor(Color.black);
            return;
        }

        Opponent oppponent = opponentRegistry.GetOpponentById(ownerId);

        Color color = oppponent != null ? oppponent.Data.Color : Color.black;

        ZoneView.ChangeColor(color);
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


    // Variant 1 External Creation 
    // SummonCreatureOperation handle logic + creates SummonVisualTask to push in visualManager
    // SummonVisualTask handle CreatureCard to creaturePresenter and position Creature to place of CreatureCard
    public void AddCreatureVisual(CreaturePresenter creaturePresenter) {
        CreatureView creatureView = creaturePresenter.CreatureView;

        // Додаємо візуально через ZoneView
        ZoneView.SummonCreatureView(creatureView);
    }

    // Variant 2 Direct React on model changes
    private void HandleCreatureSummoned(Creature creature) {
        CreaturePresenter creaturePresenter = CreateCreatureWithZoneView(creature);
        CreatureView creatureView = creaturePresenter.CreatureView;

        ZoneView.UpdateSummonedCount(Zone.Creatures.Count);
        ZoneView.SummonCreatureView(creatureView);
    }

    // Variant 2.1 Automatical registration + CreatureView from pool
    [Inject] IUnitSpawner<Creature, CreatureView, CreaturePresenter> creatureSpawner;
    private CreaturePresenter CreateCreatureWithUnitSpawner(Creature creature) {
        CreaturePresenter creaturePresenter = creatureSpawner.SpawnUnit(creature); // Automatical registration + pool view creation
        CreatureView creatureView = creaturePresenter.CreatureView;
        creatureView.gameObject.SetActive(false); // we also hide it mannually to be summoned in visual queue
        return creaturePresenter;
    }

    // Variant 2.1 Manual registration + CreatureView from ZoneView
    [Inject] IUnitRegistry unitRegistry;
    [Inject] IPresenterFactory presenterFactory;
    private CreaturePresenter CreateCreatureWithZoneView(Creature creature) {
        CreatureView creatureView = ZoneView.CreateCreatureView(); // automatically hidden by ZoneView
        CreaturePresenter creaturePresenter = presenterFactory.CreatePresenter<CreaturePresenter>(creature, creatureView);
        unitRegistry.Register(creaturePresenter); // we will also need to register it
        
        return creaturePresenter;
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
