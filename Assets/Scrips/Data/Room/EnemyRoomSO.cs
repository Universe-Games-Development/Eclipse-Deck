using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyRoom", menuName = "Map/EnemyRoom")]
public class EnemyRoomSO : RoomSO {
    public EnemySO enemy;
    public List<RewardSO> rewards;
}