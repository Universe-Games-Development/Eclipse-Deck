using UnityEngine;

public class FieldPool : BasePool<FieldController> {

    public FieldPool(FieldController prefab, Transform parent, int maxSize = 50, bool collectionCheck = false) : base(prefab, parent, maxSize, collectionCheck) {
    }

    protected override FieldController CreateObject() {
        FieldController fieldController = Object.Instantiate (prefab, defaultParent);
        fieldController.SetPool(this); // Assuming FieldController has this method
        return fieldController;
    }

    protected override void OnReturnToPool(FieldController fieldController) {
        if (fieldController != null) {
            fieldController.Reset(); // Your custom reset logic
        }
        base.OnReturnToPool(fieldController); // Call the base implementation to deactivate the object
    }

    protected override void OnDestroyObject(FieldController fieldController) {
        if (fieldController != null) {
            fieldController.Reset(); // Your custom reset logic before destroying
        }
        base.OnDestroyObject(fieldController); // Call the base implementation to destroy the object
    }
}