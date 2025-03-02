using UnityEngine;

public class FieldPool : BasePool<FieldPresenter> {

    public FieldPool(FieldPresenter prefab, Transform parent, int maxSize = 50, bool collectionCheck = false) : base(prefab, parent, maxSize, collectionCheck) {
    }

    protected override FieldPresenter CreateObject() {
        FieldPresenter fieldController = Object.Instantiate (prefab, defaultParent);
        fieldController.SetPool(this); // Assuming FieldController has this method
        return fieldController;
    }

    protected override void OnReturnToPool(FieldPresenter fieldController) {
        if (fieldController != null) {
            fieldController.Reset(); // Your custom reset logic
        }
        base.OnReturnToPool(fieldController); // Call the base implementation to deactivate the object
    }

    protected override void OnDestroyObject(FieldPresenter fieldController) {
        if (fieldController != null) {
            fieldController.Reset(); // Your custom reset logic before destroying
        }
        base.OnDestroyObject(fieldController); // Call the base implementation to destroy the object
    }
}