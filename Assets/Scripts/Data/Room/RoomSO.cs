using System.Collections.Generic;
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

public class Room {
    public RoomData data;

    public Room(RoomData roomData) {
        data = roomData;
    }
}

public class RoomFactory {
    private RoomDataRandomizer generator;
    public RoomFactory(List<RoomData> roomsFillers, System.Random rand) {
        generator = new RoomDataRandomizer(roomsFillers, rand);
    }

    // Soon to be implemented TreasureRooms, Shops, EnemyRooms and BossRooms
    public Room CreateRoom() {
        RoomData roomData = generator.GetRandomRoomData();
        return new Room(roomData);
    }
}