using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewAltarRoom", menuName = "Map/AltarRoom")]
public class AltarRoomSO : RoomData {
    public List<RewardSO> rewards;
}