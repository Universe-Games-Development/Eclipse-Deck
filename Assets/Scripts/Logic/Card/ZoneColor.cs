using UnityEngine;

public class ZoneColor : MonoBehaviour
{
    [SerializeField] ZonePresenter zonePresenter;
    [SerializeField] Renderer zoneRenderer;

    private void Awake() {
        if (zoneRenderer == null) {
            zoneRenderer = GetComponent<Renderer>();
        }
    }

    private void Start() {
        TryChangeColorToOwner();
    }

    private void TryChangeColorToOwner() {
        if (zonePresenter == null) {
            Debug.LogWarning("ZonePresenter is not assigned.");
            return;
        }
        if (zonePresenter.Owner == null) {
            Debug.LogWarning("ZonePresenter owner is not assigned.");
            return;
        }
        BoardPlayer owner = zonePresenter.Owner;
        if (owner.Character == null) {
            Debug.LogWarning("Character Data not assigned");
            return;
        }

        Color ownerColor = owner.Character.Data.Color;
        zoneRenderer.material.color = ownerColor;
    }
}
