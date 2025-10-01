using UnityEngine;

public class Unit3DInputProvider : InteractiveUnitInputProviderBase {
    [SerializeField] private Collider myCollider;

    protected override bool InitializeCollider() {
        if (myCollider == null) {
            myCollider = GetComponent<Collider>();
        }
        return myCollider != null;
    }

    private void OnMouseDown() {
        RaiseClicked();
    }

    private void OnMouseEnter() {
        RaiseCursorEnter();
    }

    private void OnMouseExit() {
        RaiseCursorExit();
    }

    protected override void UpdateColliderState(bool enabled) {
        myCollider.enabled = enabled;
    }

    
}
