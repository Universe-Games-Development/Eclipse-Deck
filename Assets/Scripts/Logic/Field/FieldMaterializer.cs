using UnityEngine;

public class FieldMaterializer : MonoBehaviour {
    [Header("CreatureOccupy emission colors")]
    [SerializeField] private Color occupyColor = Color.red;
    [SerializeField] private Color freeColor = Color.green;
    [SerializeField] private Color emptyColor = Color.gray;
    [SerializeField] private Color hoverEmissionColor;

    [Header("Type colors")]
    [SerializeField] private Color attackColor = Color.red;
    [SerializeField] private Color supportColor = Color.green;

    [Header("Opponents privacy colors")]
    [SerializeField] private Color enemyColor = Color.red;
    [SerializeField] private Color playerColor = Color.green;


    [SerializeField] private string emissiveColorName = "_EmissionColor";
    [SerializeField] private float defaultHighlightIntensity = 1f;
    [SerializeField] private float hoverHighlightIntensity = 0;

    [SerializeField] private MeshRenderer meshRenderer;

    private MaterialPropertyBlock propBlock;

    private void Awake() {
        meshRenderer = GetComponentInChildren<MeshRenderer>();
        if (meshRenderer == null) {
            Debug.LogError("MeshRenderer not found in children!");
            return;
        }

        propBlock = new MaterialPropertyBlock();
        meshRenderer.GetPropertyBlock(propBlock);
        hoverEmissionColor = freeColor;
        hoverHighlightIntensity = defaultHighlightIntensity;
    }

    public void UpdateColorBasedOnType(FieldType newType) {
        Color typeColor = Color.cyan;
        switch (newType) {
            case FieldType.Attack:
                typeColor = attackColor;
                break;
            case FieldType.Support:
                typeColor = supportColor;
                break;
            case FieldType.Empty:
                typeColor = emptyColor;
                break;
            default:
                typeColor = emptyColor;
                break;
        }
        propBlock.SetColor("_Color", typeColor);
        meshRenderer.SetPropertyBlock(propBlock);
    }


    public void UpdateColorBasedOnOwner(BoardPlayer opponent) {
        Color ownerColor = opponent is Player ? playerColor : enemyColor;
        propBlock.SetColor("_BaseColor", ownerColor); // color of middle ground
        meshRenderer.SetPropertyBlock(propBlock);

    }

    public void ToggleHovered(bool isOn) {
        propBlock.SetFloat("_EmissionIntensity", isOn ? hoverHighlightIntensity : 0);
        meshRenderer.SetPropertyBlock(propBlock);
    }


    public void UpdateOccupyEmission(Creature creature = null) {
        bool isOccupied = creature != null;

        Color hoverEmissionColor = isOccupied ? occupyColor : freeColor;
        propBlock.SetColor(emissiveColorName, hoverEmissionColor);

        if (creature != null) {
            int attackValue = creature.Attack.CurrentValue;
            hoverHighlightIntensity = defaultHighlightIntensity * attackValue;
        } else {
            hoverHighlightIntensity = defaultHighlightIntensity;
        }

        // Check if we highlight now
        float currentIntensity = propBlock.GetFloat("_EmissionIntensity");
        if (currentIntensity != 0) {
            propBlock.SetFloat("_EmissionIntensity", hoverHighlightIntensity);
        }
        meshRenderer.SetPropertyBlock(propBlock);
    }

    public void Reset() {
        propBlock.SetColor("_Color", Color.grey);
        propBlock.SetFloat("_EmissionIntensity", 0);
        if (meshRenderer != null) {
            meshRenderer.SetPropertyBlock(propBlock);
        }
    }
}