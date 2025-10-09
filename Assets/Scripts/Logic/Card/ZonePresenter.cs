using Cysharp.Threading.Tasks;
using System;
using System.Linq;
using UnityEngine;
using Zenject;

public class ZonePresenter : InteractablePresenter, IDisposable {
    public Zone Zone;
    public ZoneView ZoneView;

    [Inject] private IUnitRegistry _unitRegistry;
    [Inject] private IVisualManager _visualManager;

    public ZonePresenter(Zone zone, ZoneView zoneView) : base(zone, zoneView) {
        Zone = zone;
        ZoneView = zoneView;

        Zone.OnChangedOwner += HandleOwnerChanged;

        Zone.OnCreaturePlaced += HandleCreaturePlacement;
        Zone.OnCreatureRemoved += HandleCreatureRemove;
        Zone.ONMaxCreaturesChanged += HandleSizeUpdate;

        ZoneView.OnRemoveDebugRequest += TryRemoveCreatureDebug;


        HandleOwnerChanged(Zone.OwnerId);
        
        HandleSizeUpdate(Zone.MaxCreatures);

        foreach (var creature in Zone.Creatures) {
            HandleCreaturePlacement(creature);
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

    private void HandleCreaturePlacement(Creature creature) {
        ZoneView.UpdateSummonedCount(Zone.Creatures.Count);

        // Комбінуємо додавання View і перерозподіл в один UniTask
        var addTask = new UniversalVisualTask(
            async () => {
                await AddCreatureView(creature);
                await ZoneView.RearrangeCreatures();
            },
            "Add Creature View & Rearrange"
        );

        _visualManager.Push(addTask);
    }

    private async UniTask AddCreatureView(Creature creature) {
        // Find presenter here because queued summonVisualTask creatures Creature Presenter not immediately after Creature creation
        CreaturePresenter creaturePresenter = _unitRegistry.GetPresenter<CreaturePresenter>(creature);
        if (creaturePresenter != null) {
            await ZoneView.AddCreatureView(creaturePresenter.CreatureView, animateToPosition: false);
        }
    }

    private void HandleCreatureRemove(Creature creature) {
        ZoneView.UpdateSummonedCount(Zone.Creatures.Count);

        if (!_unitRegistry.TryGetPresenterByModel<CreaturePresenter>(creature, out var presenter)) {
            Debug.LogWarning("Failed to find presenter for : " + creature);
            return;
        }

        _unitRegistry.Unregister(presenter);
        var view = presenter.CreatureView;

        UniversalVisualTask universalVisualTask = new UniversalVisualTask(
            ZoneView.RemoveCreatureView(view),
            "Remove Creature View"
        );
        _visualManager.Push(universalVisualTask);
    }

    public void RearrangeCreatures() {
        UniversalVisualTask universalVisualTask = new UniversalVisualTask(
            ZoneView.RearrangeCreatures(),
            "Update Zone Creatures Layout"
        );
        _visualManager.Push(universalVisualTask);
    }

    public override void Dispose() {
        base.Dispose();
        Zone.OnChangedOwner -= HandleOwnerChanged;
        ZoneView.OnRemoveDebugRequest -= TryRemoveCreatureDebug;
        Zone.OnCreaturePlaced -= HandleCreaturePlacement;
        Zone.OnCreatureRemoved -= HandleCreatureRemove;
        Zone.ONMaxCreaturesChanged -= HandleSizeUpdate;
    }
}
