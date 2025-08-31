using System;
using UnityEngine;

public class ZonePresenter : UnitPresenter {
    public Zone Zone;
    public Zone3DView Zone3DView;
    [SerializeField] public BoardPlayer Owner;

    private void Start() {
        Zone = new Zone();
        Zone.ChangeOwner(Owner);
        Zone.OnCreatureSpawned += HandleCreatureSpawned;
    }

    private void HandleCreatureSpawned() {
        Zone3DView.UpdateSummonedCount(Zone.GetCreaturesCount());
    }

    public void Initialize(Zone3DView zone3DView, Zone zone) {
        Zone3DView = zone3DView;
        Zone = zone;
    }

    public override UnitModel GetInfo() {
        return Zone;
    }

    public override BoardPlayer GetPlayer() {
        return Owner;
    }

    public void SpawnCreture(CreatureCard creatureCard) {
        Zone.SpawnCreture(creatureCard);
    }
}
