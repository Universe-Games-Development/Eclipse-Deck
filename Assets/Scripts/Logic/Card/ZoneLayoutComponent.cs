using UnityEngine;

public class ZoneLayoutComponent : LinearLayoutComponent<CreatureView> {
    public Vector3 CalculateRequiredSize(int maxCreatures) {
        return new Vector3(defaultItemSize.x * maxCreatures, defaultItemSize.y, defaultItemSize.z);
    }
}