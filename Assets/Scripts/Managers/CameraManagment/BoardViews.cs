using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;

public class BoardViews : MonoBehaviour {
    private Dictionary<CameraState, CinemachineCamera> _cameras;
    [SerializeField] private List<CameraStateMapping> _cameraMappings = new();

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

    [SerializeField] private CameraManager cameraManager;
    private bool isApplicationFocused = true;

    [SerializeField] private bool isDebugMode = false;  // ����� ��� ����������� ������ ������������
    [SerializeField] public CameraState currentState;
    private Vector3 _lastMousePosition;

    private void Awake() {
        _cameras = _cameraMappings.ToDictionary(m => m.State, m => m.Camera);


        CalculateThresholds();
    }

    void Update() {
        if (cameraManager == null || !isApplicationFocused) return;
        if (Input.mousePosition == _lastMousePosition) return;
        _lastMousePosition = Input.mousePosition;

        if (isDebugMode) {
            CalculateThresholds();
        }

        CameraState chosenState = CameraState.Middle;

        bool isInUpperZone = _lastMousePosition.y > upperEnterThreshold;
        bool isInLowerZone = _lastMousePosition.y < lowerEnterThreshold;

        if (isInUpperZone) {
            chosenState = CameraState.Top;
        } else if (isInLowerZone) {
            chosenState = CameraState.Bottom;
        } else if (currentState == CameraState.Top && _lastMousePosition.y < upperExitThreshold) {
            chosenState = CameraState.Middle;
        } else if (currentState == CameraState.Bottom && _lastMousePosition.y > lowerExitThreshold) {
            chosenState = CameraState.Middle;
        }

        if (chosenState == currentState) return;
        currentState = chosenState;
        cameraManager.SwitchCamera(_cameras[chosenState]);
    }

    private void CalculateThresholds() {
        float screenHeight = Screen.height;
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

public enum CameraState { Top, Middle, Bottom };

[System.Serializable]
public class CameraStateMapping {
    public CameraState State;
    public CinemachineCamera Camera;
}