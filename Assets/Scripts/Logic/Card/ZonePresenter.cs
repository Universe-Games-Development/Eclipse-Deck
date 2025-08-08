using UnityEngine;

public class ZonePresenter : MonoBehaviour, IGameUnitProvider {
    public Zone Zone;
    public Zone3DView Zone3DView;
    private void Start() {
        Zone = new Zone();
    }

    public void Initialize(Zone3DView zone3DView, Zone zone) {
        Zone3DView = zone3DView;
        Zone = zone;
        gameObject.GetComponent<IGameUnitProvider>();
    }

    public GameUnit GetUnit() {
        return Zone;
    }
}
