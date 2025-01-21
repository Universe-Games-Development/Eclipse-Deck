using Cysharp.Threading.Tasks.Triggers;
using UnityEngine;

public enum FieldType {
    Support,
    Attack
}

public class FieldController : MonoBehaviour {

    private Field field;
    [SerializeField] public FieldUI fieldUI;


    private FieldMaterializer fieldMaterializer;
    private Levitator levitator;

    bool isInteractable = false;

    private void Awake() {
        fieldMaterializer = GetComponentInChildren<FieldMaterializer>();
        levitator = GetComponentInChildren<Levitator>();
    }

    public void InitializeLevitator(Vector3 initialPosition) {
        transform.position = initialPosition;
        if (levitator != null) {
            levitator.UpdateInitialPosition(transform.position);
            levitator.FlyToInitialPosition();
            levitator.OnFall += () => SetInteractable(true);
        }
    }

    public void Initialize(Field field) {
        fieldMaterializer.Initialize(field);

        this.field = field;
        field.OnRemoval += Remove;
    }

    private void Remove(Field field) {
        if (field != this.field) {
            Debug.LogWarning("Trying to remove by wrong field");
        }
        SetInteractable(false);
        field.OnRemoval -= Remove;
        levitator.FlyAway();
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
