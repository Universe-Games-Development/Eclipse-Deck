using UnityEngine;

public class CameraSwitcher : MonoBehaviour {
    private float screenHeight;

    [Header("Thresholds in Percentages")]
    [SerializeField, Range(0f, 100f)]
    private float upperEnterThresholdPercent = 66.7f; // ³������ ��� ����� � ������ ����
    [SerializeField, Range(0f, 100f)]
    private float upperExitThresholdPercent = 50f;   // ³������ ��� ������ � ������� ����

    [SerializeField, Range(0f, 100f)]
    private float lowerEnterThresholdPercent = 33.3f; // ³������ ��� ����� � ����� ����
    [SerializeField, Range(0f, 100f)]
    private float lowerExitThresholdPercent = 50f;   // ³������ ��� ������ � ������ ����

    private float upperEnterThreshold;  // ��������� �������� ������ � �������
    private float upperExitThreshold;   // ��������� �������� ������ � �������
    private float lowerEnterThreshold;  // ��������� �������� ������ � �������
    private float lowerExitThreshold;   // ��������� �������� ������ � �������

    private CameraManager cameraManager;
    private bool isApplicationFocused = true;

    [SerializeField]
    private bool isDebugMode = false;  // ����� ��� ����������� ������ ������������

    private void Awake() {
        cameraManager = GetComponent<CameraManager>();

        // �������� ������ ������
        screenHeight = Screen.height;

        // ����������� ������ �� ����� �������
        CalculateThresholds();
    }

    void Update() {
        if (cameraManager == null || !isApplicationFocused) return;

        // ����������, �� ��� � �������� �����
        Vector3 mousePosition = Input.mousePosition;

        if (isDebugMode) {
            // ����������� ������ � ����� ������������
            CalculateThresholds();
        }

        // ��������� ������� ���� �� ���������� ������ ����� CameraManager
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
