using UnityEngine;
using Zenject;

public enum FieldType {
    Support,
    Attack
}

public class FieldVisual : MonoBehaviour {

    [SerializeField] public FieldUI fieldUI;
    [SerializeField] private MeshRenderer meshRenderer;

    [Inject] UIManager uIManager;

    [SerializeField] private Material supportMaterialField;
    [SerializeField] private Material attackMaterialField;

    private Field field;

    private void Awake() {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    private void Initialize(Field field) {
        this.field = field;
    }

    private void UpdateMaterialBasedOnType(FieldType newType) {
        if (meshRenderer != null) {
            if (newType == FieldType.Support) {
                if (supportMaterialField)
                    meshRenderer.material = supportMaterialField;
            } else {
                meshRenderer.material = attackMaterialField;
            }
        }
    }
    [Inject] protected UIManager uiManager;

    void OnMouseEnter() {
        uiManager.ShowTip(field.GetInfo());
    }
}