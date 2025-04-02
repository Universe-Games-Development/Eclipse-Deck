using UnityEngine;

[CreateAssetMenu(fileName = "NewRoom", menuName = "Map/Room")]
public class RoomData : ScriptableObject, IWeightable {
    public string Name;
    public GameObject ViewPrefab;
    public Color roomColor;
    public float spawnChance;
    public float SpawnChance => spawnChance;

    private void OnValidate() {
        if (spawnChance == 0) {
            spawnChance = 0.01f;
        }
    }
}
