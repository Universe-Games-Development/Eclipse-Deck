using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewTreasureRoom", menuName = "Map/TreasureRoom")]
public class TreasureRoomSO : RoomData {
    public List<RewardSO> rewards;
}