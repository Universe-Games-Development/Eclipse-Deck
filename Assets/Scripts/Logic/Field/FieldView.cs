using Cysharp.Threading.Tasks;
using UnityEngine;

public class FieldView : MonoBehaviour {
    [SerializeField] private Transform creatureSpawnPoint;
    [SerializeField] private FieldMaterializer materializer;
    [SerializeField] private Levitator levitator;
    [SerializeField] private FieldUI fieldUI;

    private FieldPool pool;
    private bool isInteractable = false;

    public void SetPool(FieldPool pool) {
        this.pool = pool;
    }

    public void Initialize() {
        if (levitator != null && levitator.isActiveAndEnabled) {
            levitator.OnFall += () => SetInteractable(true);
            levitator.FlyToInitialPosition();
        }
    }

    public void UpdateCreatureVisuals(Creature creature) {
        materializer.UpdateOccupyEmission(creature);
    }

    public void UpdateOwnerVisuals(Opponent opponent) {
        materializer.UpdateColorBasedOnOwner(opponent);
    }

    public void UpdateTypeVisuals(FieldType type) {
        materializer.UpdateColorBasedOnType(type);
    }

    public void SetInteractable(bool value) {
        isInteractable = value;
    }

    public Transform GetCreaturePlace() {
        return creatureSpawnPoint;
    }

    public async UniTask RemoveWithAnimation() {
        await levitator.FlyAwayWithCallback();
        ReturnToPool();
    }

    public void ReturnToPool() {
        if (pool != null)
            pool.Release(this);
    }

    public void Reset() {
        levitator.Reset();
        materializer.Reset();
        isInteractable = false;
    }

    // Mouse interaction handlers
    private void OnMouseEnter() {
        if (isInteractable) {
            levitator.ToggleLevitation(true);
            materializer.ToggleHovered(true);
        }
    }

    private void OnMouseExit() {
        if (isInteractable) {
            levitator.ToggleLevitation(false);
            materializer.ToggleHovered(false);
        }
    }
}
