using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StartMap", menuName = "Map/StartMap")]
public class MapSO : ScriptableObject {
    public List<RoomSO> Rooms;
}
