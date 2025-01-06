using UnityEngine;

[CreateAssetMenu(fileName = "NewRoom", menuName = "Map/Room")]
public class RoomSO : ScriptableObject {
    public string roomName;
    public GameObject Prefab; // ������ �������
    public RoomType RoomType; // ��� ������� (��������, Tutorial, Enemy, Treasure)
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
