using UnityEngine;
using UnityEngine.Pool;
using Zenject;

public class FieldPool : MonoBehaviour {
    [SerializeField] private GameObject fieldPrefab;
    [SerializeField] private Transform parentTransform;

    private ObjectPool<FieldController> fieldPool;
    [Inject] DiContainer container;

    private void Awake() {
        fieldPool = new ObjectPool<FieldController>(
            CreateField,
            OnTakeFromPool,
            OnReturnToPool,
            OnDestroyField,
            maxSize: 100
        );
    }

    private FieldController CreateField() {
        GameObject fieldObject = container.InstantiatePrefab(fieldPrefab, parentTransform);
        return fieldObject.GetComponent<FieldController>();
    }

    private void OnTakeFromPool(FieldController fieldController) {
        if (fieldController != null) {
            fieldController.gameObject.SetActive(true);
        }
    }

    private void OnReturnToPool(FieldController fieldController) {
        if (fieldController != null) {
            fieldController.Reset();
            fieldController.gameObject.SetActive(false);
        }
    }

    private void OnDestroyField(FieldController fieldController) {
        if (fieldController != null) {
            fieldController.Reset();
            Destroy(fieldController.gameObject);
        }
    }

    public FieldController GetField(Field fieldData, Vector3 position) {
        FieldController fieldController = fieldPool.Get();
        if (fieldController != null) {
            fieldController.transform.localPosition = position;
            fieldController.gameObject.name = $"Field {fieldData.row} / {fieldData.column}";
            fieldController.Initialize(fieldData);
            fieldController.InitializeLevitator(position);
        }
        return fieldController;
    }

    public void ReleaseField(FieldController fieldController) {
        if (fieldController != null) {
            fieldPool.Release(fieldController);
        }
    }
}
