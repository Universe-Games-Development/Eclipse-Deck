using UnityEngine;

public class FieldMaterializer : MonoBehaviour {
    [SerializeField] private Color attackColor = Color.red;
    [SerializeField] private Color supportColor = Color.green;
    [SerializeField] private Color emptyColor = Color.gray;

    [SerializeField] private Color enemyColor = Color.red;
    [SerializeField] private Color playerColor = Color.green;

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
        UpdateColorBasedOnType(field.FieldType);
        UpdateColorBasedOnOwner(field.Owner);
        field.OnChangedOwner += UpdateColorBasedOnOwner;
        field.OnChangedType += UpdateColorBasedOnType;
    }

    public void UpdateColorBasedOnType(FieldType newType) {
        if (meshRenderer != null) {
            switch (newType) {
                case FieldType.Attack:
                    originalColor = attackColor;
                    break;
                case FieldType.Support:
                    originalColor = supportColor;
                    break;
                case FieldType.Empty:
                    originalColor = emptyColor;
                    break;
                default:
                    originalColor = emptyColor;
                    break;
            }
            propBlock.SetColor("_Color", originalColor);
            meshRenderer.SetPropertyBlock(propBlock);
        }
    }


    public void UpdateColorBasedOnOwner(Opponent opponent) {
        if (meshRenderer != null) {
            originalColor = opponent is Player ? playerColor : enemyColor;
            propBlock.SetColor("_BaseColor", originalColor);
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

    public void Reset() {
        if (field != null) {
            field.OnChangedType -= UpdateColorBasedOnType;
            field.OnChangedOwner -= UpdateColorBasedOnOwner;
            field = null;
        }
        
        originalColor = default(Color);
        propBlock.SetColor("_Color", originalColor);
        propBlock.SetFloat("_EmissionIntensity", originalEmissionIntensity);
        if (meshRenderer != null) {
            meshRenderer.SetPropertyBlock(propBlock);
        }
    }


    private void OnDestroy() {
        if (field != null) {
            field.OnChangedType -= UpdateColorBasedOnType;
            field.OnChangedOwner -= UpdateColorBasedOnOwner;
        }
    }
}