using System;
using UnityEngine;
using Zenject;

public class ZonePresenter : UnitPresenter {
    public Zone Zone;
    public Zone3DView View;
    [SerializeField] public BoardPlayer Owner;
    [Inject] ICreatureSpawnService _spawnService;

    private void Start() {
        Zone = new Zone();
        Zone.ChangeOwner(Owner);
        Zone.OnCreatureSpawned += HandleCreatureSpawned;
    }

    public void Initialize(Zone3DView zone3DView, Zone zone) {
        View = zone3DView;
        Zone = zone;
    }

    public override UnitModel GetInfo() {
        return Zone;
    }

    public override BoardPlayer GetPlayer() {
        DebugLog($"Getting owner: {Owner?.name}");
        return Owner;
    }

    private void HandleCreatureSpawned() {
        DebugLog("Creature spawned event received");
        View.UpdateSummonedCount(Zone.GetCreaturesCount());
    }

    public bool PlaceCreature(CreaturePresenter creaturePresenter, Vector3 spawnPosition) {
        DebugLog($"Spawning creature card: {creaturePresenter}");
        Zone.PlaceCreature(creaturePresenter.Creature);
        return true;
    }

    public override void Highlight(bool enable) {
        DebugLog($"Setting highlight to: {enable}");
        View.Highlight(enable);
    }
}
