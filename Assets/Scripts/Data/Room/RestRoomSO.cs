using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RestRoom", menuName = "Map/RestRoom")]
public class RestRoomSO : RoomData {
    public List<RewardSO> rewards;
}