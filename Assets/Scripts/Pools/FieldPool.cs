using UnityEngine;

public class FieldPool : BasePool<FieldView> {

    public FieldPool(FieldView prefab, Transform parent, int maxSize = 50, bool collectionCheck = false) : base(prefab, parent, maxSize, collectionCheck) {
    }

    protected override FieldView CreateObject() {
        FieldView fieldView = Object.Instantiate (prefab, defaultParent);
        fieldView.SetPool(this); // Assuming FieldController has this method
        return fieldView;
    }

    protected override void OnReturnToPool(FieldView fieldView) {
        if (fieldView != null) {
            fieldView.Reset(); // Your custom reset logic
        }
        base.OnReturnToPool(fieldView); // Call the base implementation to deactivate the object
    }

    protected override void OnDestroyObject(FieldView fieldView) {
        if (fieldView != null) {
            fieldView.Reset(); // Your custom reset logic before destroying
        }
        base.OnDestroyObject(fieldView); // Call the base implementation to destroy the object
    }
}