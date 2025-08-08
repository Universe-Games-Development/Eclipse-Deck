using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour {
    public event Action<InputActionMap> OnMainMapDisabled;
    public event Action<InputActionMap> OnMainMapSwitched;

    public InputSystem_Actions inputAsset { get; private set; }

    public InputActionMap PlayerMap { get; private set; }
    public InputActionMap BoardPlayer { get; private set; }
    public InputActionMap UIMap { get; private set; }

    public InputActionMap CurrentMap => _mainMap;
    public InputActionMap PreviousMap => _previousMap;

    private InputActionMap _mainMap;
    private InputActionMap _previousMap;
    private bool _isInitialized;

    private void Awake() {
        if (_isInitialized) return;
        Initialize();
    }

    private void Initialize() {
        _isInitialized = true;
        inputAsset = new InputSystem_Actions();
        inputAsset.Disable();

        PlayerMap = inputAsset.Player;
        BoardPlayer = inputAsset.BoardPlayer;
        UIMap = inputAsset.UI;

        SwitchActionMap(BoardPlayer);
    }

    public void SwitchActionMap(InputActionMap map) {
        if (map == null || _mainMap == map) return;

        if (_mainMap != null) {
            OnMainMapDisabled?.Invoke(_mainMap);
            _previousMap = _mainMap;
            _mainMap.Disable();
        }

        _mainMap = map;
        _mainMap.Enable();
        OnMainMapSwitched?.Invoke(_mainMap);
    }

    public void ReturnToPreviousMode() {
        if (_previousMap != null) {
            SwitchActionMap(_previousMap);
        }
    }

    public InputAction GetAction(string actionId) {
        var action = inputAsset?.FindAction(actionId);
        if (action == null) {
            Debug.LogError($"Action '{actionId}' not found in input asset!");
        }
        return action;
    }

    private void OnDisable() {
        if (_mainMap?.enabled == true) {
            OnMainMapDisabled?.Invoke(_mainMap);
            _mainMap.Disable();
        }
        inputAsset?.Disable();
    }

    private void OnDestroy() {
        inputAsset?.Dispose();
    }
}