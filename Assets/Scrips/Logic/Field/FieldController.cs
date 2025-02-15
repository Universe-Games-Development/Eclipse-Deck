using Cysharp.Threading.Tasks;
using UnityEngine;
public class FieldController : MonoBehaviour, ILogicHolder<Field> {
    public FieldType type;
    public string owner;

    public Field Logic { get; private set; }

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
        Logic = field;
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
        if (isInteractable && Logic.Owner != null && Logic.Owner is Player) {
            levitator.ToggleLevitation(true);
            fieldMaterializer.ToggleHighlight(true);
        }
    }

    private void OnMouseExit() {
        if (isInteractable && Logic.Owner != null && Logic.Owner is Player) {
            levitator.ToggleLevitation(false);
            fieldMaterializer.ToggleHighlight(false);
        }
    }

    public void Reset() {
        levitator.Reset();
        fieldMaterializer.Reset();

        isInteractable = false;
        Logic = null;
    }
}
