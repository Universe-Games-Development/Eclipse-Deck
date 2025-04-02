public interface IRandomRoomFactory {
    Room GetRoom(DungeonGraph graph, DungeonNode node);
    void UpdateRoomData(LocationRoomsData currentLevelData);
}