using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyRoom", menuName = "Map/EnemyRoom")]
public class EnemyRoomSO : RoomData {
    public EnemySO enemy;
    public List<RewardSO> rewards;
}