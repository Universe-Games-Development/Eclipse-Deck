using UnityEngine;
using Zenject;

public abstract class ActivityData : ScriptableObject, IWeightable {
    public string Name;
    public float spawnChance;

    public float SpawnChance => spawnChance;

    public abstract RoomActivity CreateActivity(DiContainer diContainer);

    private void OnValidate() {
        if (spawnChance == 0) {
            spawnChance = 0.01f;
        }
    }
}
