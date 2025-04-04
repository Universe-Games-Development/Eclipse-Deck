using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "OverallLocations", menuName = "Map/OverallLocations")]
public class LocationsData : ScriptableObject {
    public List<LocationData> locationDatas;
}
