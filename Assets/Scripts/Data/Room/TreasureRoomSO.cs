using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewTreasureRoom", menuName = "Map/TreasureRoom")]
public class TreasureRoomSO : RoomSO {
    public List<RewardSO> rewards;
}