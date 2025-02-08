using Cysharp.Threading.Tasks;
using UnityEngine;

public class FieldController : MonoBehaviour {
    public FieldType type;
    public string owner;

    private Field field;
    [SerializeField] public FieldUI fieldUI;


    [SerializeField] private FieldMaterializer fieldMaterializer;
    [SerializeField] public Levitator levitator;

    private FieldPool pool;

    bool isInteractable = false;

    public void InitializeLevitator(Vector3 initialPosition) {
        transform.position = initialPosition;
        if (levitator != null) {
            levitator.FlyToInitialPosition();
            levitator.OnFall += () => SetInteractable(true);
        }
    }

    public void SetPool(FieldPool pool) {
        this.pool = pool;
    }

    public void Initialize(Field field) {
        
        if (field == null) {
            Debug.LogError("null field data");
            return;
        }
        this.field = field;
        type = field.FieldType;
        fieldMaterializer.Initialize(field);
        if (field.Owner != null) {
            owner = field.Owner.Name;
        }
    }

    private void SetInteractable(bool value) {
        isInteractable = value;
    }

    public async UniTask RemoveController() {
        await levitator.FlyAwayWithCallback();
        Reset();
        ReturnToPool();
    }

    public void ReturnToPool() {
        pool.Release(this);
    }

    private void OnMouseEnter() {
        if (isInteractable && field.Owner != null && field.Owner is Player) {
            levitator.ToggleLevitation(true);
            fieldMaterializer.ToggleHighlight(true);
        }
    }

    private void OnMouseExit() {
        if (isInteractable && field.Owner != null && field.Owner is Player) {
            levitator.ToggleLevitation(false);
            fieldMaterializer.ToggleHighlight(false);
        }
    }

    public void Reset() {
        levitator.Reset();
        fieldMaterializer.Reset();

        isInteractable = false;
        field = null;
    }
}
