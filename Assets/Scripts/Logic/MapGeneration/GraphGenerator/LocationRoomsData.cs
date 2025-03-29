using System;
using System.Collections.Generic;
using UnityEngine;

// Stores visuals of each room for locations
[CreateAssetMenu(fileName = "DungeonLevelData", menuName = "Dungeon/DungeonLevelData")]
public class LocationRoomsData : ScriptableObject {
    public MapGenerationData mapGenerationData;
    [Header("Base Rooms")]
    public RoomData entranceRoom;
    public RoomData bossRoom;
    public RoomData exitRoom;

    [Header("Random Rooms")]
    public List<RoomData> commonRooms = new();
    public List<RoomData> specialRooms = new();

    [Header("Shop")]
    public ShopRoomData shopData;

    [Header("Altars")]
    public List<AltarRoomData> altarDatas = new();

    [Header("Enemies")]
    public List<OpponentData> enemyPool = new();

}

[Serializable]
public class ShopRoomData {
    public GameObject shopPrefab;
    public List<CardData> availableCards = new();
    public Vector2Int cardPriceRange = new(50, 100);
}

[Serializable]
public class AltarRoomData {
    public GameObject altarPrefab;
    public List<CardData> availableCards = new();
    public Vector2Int cardPriceRange = new(50, 100);
}