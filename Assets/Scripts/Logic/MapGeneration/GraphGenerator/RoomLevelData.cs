// Клас вузла залишаємо майже без змін
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomsLevelData", menuName = "MapGeneration/RoomsLevelData")]

public class RoomLevelData : ScriptableObject {
    public List<RoomData> rooms = new();
    public RoomData enterance;
    public RoomData bossLevelRoom;
}
