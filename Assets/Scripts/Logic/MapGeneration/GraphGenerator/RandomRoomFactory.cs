using System.IO;
using UnityEngine;

public interface IRoomFactory {
    Room GetRoom(DungeonGraph graph, DungeonNode node);
    void UpdateRoomData(LocationRoomsData currentLevelData);
}

public class RandomRoomFactory : IRoomFactory {
    private WeightedRandomizer<RoomData> weightedRandomizer = new();
    private LocationRoomsData roomsData;

    public RandomRoomFactory(LocationRoomsData currentLevelData) {
        UpdateRoomData(currentLevelData);
    }
    public RandomRoomFactory() {
    }

    public void UpdateRoomData(LocationRoomsData currentLevelData) {
        if (currentLevelData == null) {
            throw new InvalidDataException("currentLevelData is null");
        }
        roomsData = currentLevelData;
        weightedRandomizer.UpdateItems(roomsData.commonRooms);
    }

    public Room GetRoom(DungeonGraph graph, DungeonNode node) {
        if (roomsData == null) {
            Debug.LogWarning("roomData not initialized");
            return null;
        }
        RoomData roomData = null;
        int level = node.level;

        if (level == 0) {
            roomData = roomsData.entranceRoom;
        } else if (level == graph.GetLevelNodes().Count - 2) { // preEndLevel
            roomData = roomsData.bossRoom;
        } else if (level == graph.GetLevelNodes().Count - 1) { // endLevel
            roomData = roomsData.exitRoom;
        } else {
            roomData = weightedRandomizer.GetRandomItem();
        }


        if (roomData == null) throw new InvalidDataException("Room data is null");
        Room room = new(node, roomData);

        return room;
    }
}