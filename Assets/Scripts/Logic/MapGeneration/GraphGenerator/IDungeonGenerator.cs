using System;

public interface IDungeonGenerator {
    void ClearDungeon();
    bool GenerateDungeon(LocationRoomsData currentLevelData, out DungeonGraph dungeonGraph);
}