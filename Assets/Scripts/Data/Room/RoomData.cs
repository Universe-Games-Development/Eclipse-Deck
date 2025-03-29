using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewRoom", menuName = "Map/Room")]
public class RoomData : ScriptableObject {
    public string roomName;
    public GameObject ViewPrefab;
    public RoomType type;
    public LocationType location;
    public float spawnChance;
    public Color roomColor;

    private void OnValidate() {
        if (spawnChance == 0) {
            spawnChance = 1f;
        }
        if (roomName == null || string.IsNullOrEmpty(roomName)) {
            roomName = type.ToString();
        }
    }
}
