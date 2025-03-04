using UnityEngine;
using System;

[RequireComponent(typeof(MeshRenderer))]
public class TurnButtonView : MonoBehaviour {
    private MaterialPropertyBlock propBlock;
    [SerializeField] private Color originalColor;
    [SerializeField] private Color inactiveColor = Color.gray;

    public event Action OnTurnButtonClicked;
    private bool isEnabled = false;
    private MeshRenderer meshRenderer;

    private void Awake() {
        propBlock = new MaterialPropertyBlock();
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.GetPropertyBlock(propBlock);
    }

    public void SetInteractive(bool isActive) {
        if (isActive == isEnabled) return;
        isEnabled = isActive;

        Color color = isEnabled ? originalColor : inactiveColor;
        propBlock.SetColor("_BaseColor", color);
        propBlock.SetColor("_EmissiveColor", color);
        meshRenderer.SetPropertyBlock(propBlock);
    }

    private void OnMouseUpAsButton() {
        if (isEnabled)
            OnTurnButtonClicked?.Invoke();
    }

    private void OnMouseEnter() {
        Debug.Log("Turn Button hovered");
    }
}
