using UnityEngine;

public class FieldController : MonoBehaviour {
    public FieldType type;
    public string owner;

    private Field field;
    [SerializeField] public FieldUI fieldUI;


    [SerializeField] private FieldMaterializer fieldMaterializer;
    [SerializeField] public Levitator levitator;

    bool isInteractable = false;

    private void Awake() {
        fieldMaterializer = GetComponentInChildren<FieldMaterializer>();
        levitator = GetComponentInChildren<Levitator>();
    }

    public void InitializeLevitator(Vector3 initialPosition) {
        transform.position = initialPosition;
        if (levitator != null) {
            levitator.FlyToInitialPosition();
            levitator.OnFall += () => SetInteractable(true);
        }
    }

    public void Initialize(Field field) {
        
        if (field == null) {
            Debug.LogError("null field data");
            return;
        }
        fieldMaterializer.Initialize(field);
        this.field = field;
        type = field.Type;
        if (field.Owner != null) {
            owner = field.Owner.Name;
        }
    }


    private void SetInteractable(bool value) {
        isInteractable = value;
        if (field == null) return;
        if (value) {
            field.OnSelect += levitator.ToggleLevitation;
            field.OnSelect += fieldMaterializer.ToggleHighlight;
        } else {
            field.OnSelect -= levitator.ToggleLevitation;
            field.OnSelect -= fieldMaterializer.ToggleHighlight;
        }
    }

    public void Reset() {
        levitator.Reset();
        fieldMaterializer.Reset();

        isInteractable = false;
        if (field != null)
            field.OnSelect -= levitator.ToggleLevitation;
        field = null;
    }

    private void OnDestroy() {
        if (field != null)
            field.OnSelect -= levitator.ToggleLevitation;
    }
}
