using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour {
    public CinemachineCamera topCamera;
    public CinemachineCamera middleCamera;
    public CinemachineCamera bottomCamera;
    public CinemachineCamera startCamera;

    public CinemachineCamera activeCamera;
    private Dictionary<CameraState, CinemachineCamera> cameras = new Dictionary<CameraState, CinemachineCamera>();

    private CameraSwitcher switcher;
    private CameraSplineMover cameraSplineMover;

    [SerializeField]
    public CameraState currentState;

    private void Awake() {
        switcher = GetComponent<CameraSwitcher>();
        cameraSplineMover = GetComponent<CameraSplineMover>();

        if (topCamera == null || middleCamera == null || bottomCamera == null || startCamera == null) {
            Debug.LogError("Не всі камери призначені в інспекторі. Перевірте налаштування.");
            enabled = false;
            return;
        }

        cameras.Add(CameraState.Top, topCamera);
        cameras.Add(CameraState.Middle, middleCamera);
        cameras.Add(CameraState.Bottom, bottomCamera);
        cameras.Add(CameraState.Start, startCamera);

        if (cameraSplineMover != null) {
            cameraSplineMover.OnMovementStart += DeactivateCameraSwitcher;
            cameraSplineMover.OnMovementComplete += ActivateCameraSwitcher;
            cameraSplineMover.OnMovementComplete += () => SwitchCamera(CameraState.Middle);
        }

    }

    private void OnDestroy() {
        if (cameraSplineMover != null) {
            cameraSplineMover.OnMovementStart -= DeactivateCameraSwitcher;
            cameraSplineMover.OnMovementComplete -= ActivateCameraSwitcher;
        }
    }

    void Start() {
        // Встановлюємо початкову камеру
        SwitchCamera(CameraState.Start);
    }

    public void SwitchCamera(CameraState newState) {
        if (!cameras.ContainsKey(newState)) {
            Debug.LogError($"CameraState {newState} не знайдено в словнику.");
            return;
        }

        if (currentState == newState) {
            return;
        }

        if (activeCamera != null) {
            activeCamera.Priority = 0;
        }

        activeCamera = cameras[newState];
        activeCamera.Priority = 1;
        currentState = newState;
    }

    public void StartGame() {
        cameraSplineMover.StartCameraMovement();
    }

    private void ActivateCameraSwitcher() {
        if (switcher != null) {
            switcher.enabled = true;
        }
    }

    private void DeactivateCameraSwitcher() {
        if (switcher != null) {
            switcher.enabled = false;
        }
    }
}

public enum CameraState { Top, Middle, Bottom, Start };
