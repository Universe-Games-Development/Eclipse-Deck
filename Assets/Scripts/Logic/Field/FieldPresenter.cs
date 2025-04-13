using Cysharp.Threading.Tasks;
using UnityEngine;

public class FieldPresenter : MonoBehaviour {
    public FieldType type;
    public string ownerName;

    public Field Field;

    [SerializeField] public Transform creatureSpawnPoint;

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
        Field = field;
        type = field.FieldType;

        fieldMaterializer.UpdateColorBasedOnType(field.FieldType);
        fieldMaterializer.UpdateColorBasedOnOwner(field.Owner);

        field.OnChangedOwner += OnOwnerChanged;
        field.OnChangedType += OnTypeChanged;
        field.OnCreaturePlaced += OnOccupy;
        field.OnCreatureRemoved += OnDeOccupy;

        if (field.Owner != null) {
            ownerName = field.Owner.ToString();
        }
    }

    private void OnTypeChanged(FieldType type) {
        fieldMaterializer.UpdateColorBasedOnType(type);
    }

    private void OnOwnerChanged(Opponent opponent) {
        fieldMaterializer.UpdateColorBasedOnOwner(opponent);
    }

    private void OnOccupy(Creature creature) {
        fieldMaterializer.UpdateOccupyEmission(creature);
    }

    private void OnDeOccupy(Creature creature) {
        fieldMaterializer.UpdateOccupyEmission();
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

    public Transform GetCreaturePlace() {
        return creatureSpawnPoint;
    }

    private void OnMouseEnter() {
        if (isInteractable && Field.Owner != null && Field.Owner is Player) {
            levitator.ToggleLevitation(true);
            fieldMaterializer.ToggleHovered(true);
        }
    }

    private void OnMouseExit() {
        if (isInteractable && Field.Owner != null && Field.Owner is Player) {
            levitator.ToggleLevitation(false);
            fieldMaterializer.ToggleHovered(false);
        }
    }

    public void Reset() {
        levitator.Reset();
        fieldMaterializer.Reset();

        isInteractable = false;
        Field = null;
    }

    private void OnDestroy() {
        if (Field == null) return;

        Field.OnChangedOwner -= OnOwnerChanged;
        Field.OnChangedType -= OnTypeChanged;
        Field.OnCreaturePlaced -= OnOccupy;
        Field.OnCreatureRemoved -= OnDeOccupy;
    }

}
