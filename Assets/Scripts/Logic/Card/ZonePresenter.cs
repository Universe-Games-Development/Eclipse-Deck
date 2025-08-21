using UnityEngine;

public class ZonePresenter : MonoBehaviour, IGameUnitProvider {
    public Zone Zone;
    public Zone3DView Zone3DView;
    [SerializeField] public BoardPlayer Owner;

    private void Start() {
        Zone = new Zone();
        Zone.Owner = Owner;
    }

    public void Initialize(Zone3DView zone3DView, Zone zone) {
        Zone3DView = zone3DView;
        Zone = zone;
    }

    public GameUnit GetUnit() {
        return Zone;
    }
}
