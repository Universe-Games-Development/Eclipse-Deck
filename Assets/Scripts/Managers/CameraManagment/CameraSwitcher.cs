using UnityEngine;

public class CameraSwitcher : MonoBehaviour {
    private float screenHeight;

    [Header("Thresholds in Percentages")]
    [SerializeField, Range(0f, 100f)]
    private float upperEnterThresholdPercent = 66.7f; // Відсоток для входу у верхню зону
    [SerializeField, Range(0f, 100f)]
    private float upperExitThresholdPercent = 50f;   // Відсоток для виходу з верхньої зони

    [SerializeField, Range(0f, 100f)]
    private float lowerEnterThresholdPercent = 33.3f; // Відсоток для входу у нижню зону
    [SerializeField, Range(0f, 100f)]
    private float lowerExitThresholdPercent = 50f;   // Відсоток для виходу з нижньої зони

    private float upperEnterThreshold;  // Абсолютне значення порогу у пікселях
    private float upperExitThreshold;   // Абсолютне значення порогу у пікселях
    private float lowerEnterThreshold;  // Абсолютне значення порогу у пікселях
    private float lowerExitThreshold;   // Абсолютне значення порогу у пікселях

    private CameraManager cameraManager;
    private bool isApplicationFocused = true;

    [SerializeField]
    private bool isDebugMode = false;  // Змінна для перемикання режимів демонстрації

    private void Awake() {
        cameraManager = GetComponent<CameraManager>();

        // Зберігаємо висоту екрану
        screenHeight = Screen.height;

        // Розраховуємо пороги на основі відсотків
        CalculateThresholds();
    }

    void Update() {
        if (cameraManager == null || !isApplicationFocused) return;

        // Перевіряємо, чи гра є активним вікном
        Vector3 mousePosition = Input.mousePosition;

        if (isDebugMode) {
            // Розраховуємо пороги в режимі демонстрації
            CalculateThresholds();
        }

        // Визначаємо активну зону та перемикаємо камеру через CameraManager
        if (mousePosition.y > upperEnterThreshold) {
            cameraManager.SwitchCamera(CameraState.Top);
        } else if (mousePosition.y < lowerEnterThreshold) {
            cameraManager.SwitchCamera(CameraState.Bottom);
        } else if (cameraManager.currentState == CameraState.Top && mousePosition.y < upperExitThreshold) {
            cameraManager.SwitchCamera(CameraState.Middle);
        } else if (cameraManager.currentState == CameraState.Bottom && mousePosition.y > lowerExitThreshold) {
            cameraManager.SwitchCamera(CameraState.Middle);
        }
    }

    private void CalculateThresholds() {
        upperEnterThreshold = screenHeight * (upperEnterThresholdPercent / 100f);
        upperExitThreshold = screenHeight * (upperExitThresholdPercent / 100f);
        lowerEnterThreshold = screenHeight * (lowerEnterThresholdPercent / 100f);
        lowerExitThreshold = screenHeight * (lowerExitThresholdPercent / 100f);
    }

    private void OnEnable() {
        Application.focusChanged += OnApplicationFocusChanged;
    }

    private void OnDisable() {
        Application.focusChanged -= OnApplicationFocusChanged;
    }

    private void OnApplicationFocusChanged(bool focusStatus) {
        isApplicationFocused = focusStatus;
    }
}
