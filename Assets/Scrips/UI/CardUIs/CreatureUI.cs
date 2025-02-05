using UnityEngine;

public class CreatureUI : MonoBehaviour {
    protected RectTransform rectTransform;
    private void Awake() {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Update() {
        // Поворачиваем UI к основной камере
        if (Camera.main != null && rectTransform != null) {
            Vector3 directionToCamera = rectTransform.position - Camera.main.transform.position;
            directionToCamera.z = 0;
            rectTransform.rotation = Quaternion.LookRotation(directionToCamera);
        }
    }

    public void PositionPanelInWorld(Transform uiPosition) {
        rectTransform.position = uiPosition.position;
        rectTransform.rotation = uiPosition.rotation;
    }
}
