using UnityEngine;

[CreateAssetMenu(fileName = "NewRoom", menuName = "Map/Room")]
public class RoomSO : ScriptableObject {
    public string roomName;
    public GameObject Prefab; // Префаб комнаты
    public RoomType RoomType; // Тип комнаты (например, Tutorial, Enemy, Treasure)
}

public enum RoomType {
    Tutorial,
    Enemy,
    Treasure,
    Altar,
    Rest,
    Boss,
    Shop
}
