using UnityEngine;

[CreateAssetMenu(fileName = "NewRoom", menuName = "Map/Room")]
public class RoomData : ScriptableObject {
    public string roomName;
    public GameObject Prefab; // Префаб комнаты
    public RoomType type;
    public float baseChance; // Базовий шанс генерації
    public float repeatPenalty; // Множник зниження шансу при повторенні в одній гілці (0.5 = 50%)
    public Color roomColor; // Колір для візуалізації
}
