using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

public class CameraController : MonoBehaviour {
    [Inject] InputManager inputManager;
    private InputSystem_Actions.BoardPlayerActions bPActions;

    [Header("Cameras")]
    public CinemachineCamera mainCamera;
    public CinemachineCamera rightCamera;
    public CinemachineCamera leftCamera;
    public CinemachineCamera upCamera;
    public CinemachineCamera downCamera;

    private Vector2 currentCameraPosition = Vector2.zero; // (0,0) - основная камера
    private CinemachineCamera currentCamera;

    // Карта позиций камер
    private Dictionary<Vector2, CinemachineCamera> cameraPositionMap;

    private void Awake() {
        InitializeCameraMap();
        SwitchToCamera(mainCamera);
    }

    private void InitializeCameraMap() {
        cameraPositionMap = new Dictionary<Vector2, CinemachineCamera>
        {
            { Vector2.zero, mainCamera },           // (0, 0) - основная
            { Vector2.right, rightCamera },         // (1, 0) - правая
            { Vector2.left, leftCamera },           // (-1, 0) - левая
            { Vector2.up, upCamera },               // (0, 1) - верхняя
            { Vector2.down, downCamera }            // (0, -1) - нижняя
        };
    }

    private void OnEnable() {
        RegisterInputEvents();
    }

    private void OnDisable() {
        UnRegisterInputEvents();
    }

    private void RegisterInputEvents() {
        if (inputManager == null) return;
        if (inputManager.inputAsset == null) return;
        bPActions = inputManager.inputAsset.BoardPlayer;
        bPActions.Look.performed += OnLookPerformed;
    }

    private void UnRegisterInputEvents() {
        if (inputManager == null) return;
        if (inputManager.inputAsset == null) return;
        bPActions.Look.performed -= OnLookPerformed;
    }

    private void OnLookPerformed(InputAction.CallbackContext context) {
        Vector2 inputDirection = context.ReadValue<Vector2>();
        Debug.Log($"Look input received: {inputDirection}");

        Vector2 newCameraPosition = CalculateNewCameraPosition(inputDirection);

        if (newCameraPosition != currentCameraPosition) {
            SetCameraPosition(newCameraPosition);
        }
    }

    private Vector2 CalculateNewCameraPosition(Vector2 inputDirection) {
        if (inputDirection.x != 0)
            return new Vector2(Mathf.Round(currentCameraPosition.x) + inputDirection.x, 0);

        if (inputDirection.y != 0)
            return new Vector2(0, Mathf.Round(currentCameraPosition.y) + inputDirection.y);

        return currentCameraPosition;
    }

    private void SetCameraPosition(Vector2 newPosition) {
        if (cameraPositionMap.TryGetValue(newPosition, out CinemachineCamera newCamera)) {
            currentCameraPosition = newPosition;
            SwitchToCamera(newCamera);
            Debug.Log($"Camera position changed to: {currentCameraPosition}, Camera: {newCamera.name}");
        }
    }

    private void SwitchToCamera(CinemachineCamera newCamera) {
        if (currentCamera != null) {
            currentCamera.Priority = 0;
        }

        currentCamera = newCamera;
        currentCamera.Priority = 1;

        //Debug.Log($"Switched to camera: {currentCamera.name}");
    }

    public Vector2 GetCurrentCameraPosition() {
        return currentCameraPosition;
    }

    public CinemachineCamera GetCurrentCamera() {
        return currentCamera;
    }

    public void ResetToMainCamera() {
        SetCameraPosition(Vector2.zero);
    }

    public void SetCameraByPosition(Vector2 position) {
        SetCameraPosition(position);
    }
}