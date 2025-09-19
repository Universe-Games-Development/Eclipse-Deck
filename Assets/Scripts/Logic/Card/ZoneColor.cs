using UnityEngine;

public class ZoneColor : MonoBehaviour {
    [SerializeField] ZonePresenter zonePresenter;
    [SerializeField] Renderer zoneRenderer;
    [SerializeField] Color unAssignedColor;

    private void Awake() {
        if (zoneRenderer == null) {
            zoneRenderer = GetComponent<Renderer>();
        }
    }

    private void Start() {
        TryChangeColorToOwner();

        zonePresenter.Zone.OnChangedOwner += HandleOwnerChanged;
    }

    private void HandleOwnerChanged(Opponent opponent) {
        Color color = unAssignedColor;

        if (opponent != null && opponent.Data != null) {
            CharacterData data = opponent.Data;
            color = data.Color;
        }

        if (zonePresenter.Owner != null) {
            CharacterData data = zonePresenter.Owner.Opponent.Data;
            color = data.Color;
        }

        zoneRenderer.material.color = color;
    }

    private void TryChangeColorToOwner() {
        if (zonePresenter == null || zonePresenter.Zone == null) {
            Debug.LogWarning("ZonePresenter is not assigned.");
            return;
        }
        var owner = zonePresenter.Zone.GetPlayer(); ;
        HandleOwnerChanged(owner);
    }
}
