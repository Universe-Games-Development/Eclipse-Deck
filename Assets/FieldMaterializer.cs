using UnityEngine;

public class FieldMaterializer : MonoBehaviour {
    [SerializeField] private Color attackColor = Color.red;
    [SerializeField] private Color supportColor = Color.green;
    [SerializeField] private float highlightIntensity = 2f; // Інтенсивність підсвічування

    [SerializeField] private MeshRenderer meshRenderer;

    private Color originalColor;
    private float originalEmissionIntensity;
    private Field field;

    private MaterialPropertyBlock propBlock;

    private void Awake() {
        meshRenderer = GetComponentInChildren<MeshRenderer>();
        if (meshRenderer == null) {
            Debug.LogError("MeshRenderer not found in children!");
            return;
        }

        propBlock = new MaterialPropertyBlock();

        meshRenderer.GetPropertyBlock(propBlock);
        originalColor = propBlock.GetColor("_Color");
        originalEmissionIntensity = propBlock.GetFloat("_EmissionIntensity");
    }

    public void Initialize(Field field) {
        this.field = field;
        UpdateColorBasedOnType(field.Type);
        field.OnChangedType += UpdateColorBasedOnType;
    }

    public void UpdateColorBasedOnType(FieldType newType) {
        if (meshRenderer != null) {
            originalColor = newType == FieldType.Support ? supportColor : attackColor;
            propBlock.SetColor("_Color", originalColor);
            meshRenderer.SetPropertyBlock(propBlock);
        }
    }

    public void ToggleHighlight(bool isOn) {
        if (meshRenderer == null) {
            return;
        }

        if (isOn) {
            propBlock.SetFloat("_EmissionIntensity", highlightIntensity);
            meshRenderer.SetPropertyBlock(propBlock);
            return;
        }
        propBlock.SetFloat("_EmissionIntensity", originalEmissionIntensity);
        meshRenderer.SetPropertyBlock(propBlock);
    }

    private void OnDestroy() {
        if (field != null) {
            field.OnChangedType -= UpdateColorBasedOnType;
        }
    }
}