using System;
using UnityEngine;

public class ZoneColor : MonoBehaviour
{
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
    }

    private void TryChangeColorToOwner() {
        if (zonePresenter == null) {
            Debug.LogWarning("ZonePresenter is not assigned.");
            return;
        }
        if (zonePresenter.Owner == null) {
            //Debug.LogWarning("ZonePresenter owner is not assigned.");
        }
        BoardPlayer owner = zonePresenter.Owner;
        Color color = owner == null ? unAssignedColor : owner.Character.Data.Color;

        zoneRenderer.material.color = color;
    }
}
