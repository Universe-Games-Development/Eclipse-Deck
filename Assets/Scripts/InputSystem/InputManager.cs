using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour {
    public event Action<InputActionMap> OnMainMapDisabled;
    public event Action<InputActionMap> OnMainMapSwitched;

    public InputSystem_Actions inputAsset;

    public InputActionMap PlayerMap { get; private set; }
    public InputActionMap BoardPlayer { get; private set; }
    public InputActionMap UIMap { get; private set; }

    public InputActionMap _mainMap;
    public InputActionMap CurrentMap => _mainMap;
    public InputActionMap previousMap;


    private void Awake() {
        inputAsset = new InputSystem_Actions();
        inputAsset.Disable();

        PlayerMap = inputAsset.Player;
        BoardPlayer = inputAsset.BoardPlayer;
        UIMap = inputAsset.UI;


        SwitchActionMap(BoardPlayer);
    }

    private void OnDisable() {
        if (_mainMap != null) {
            OnMainMapDisabled?.Invoke(_mainMap);
            _mainMap.Disable();
        }

        inputAsset.Disable();
    }

    public void SwitchActionMap(InputActionMap map) {
        if (_mainMap == map) return;

        if (_mainMap != null) {
            OnMainMapDisabled?.Invoke(_mainMap);
            previousMap = _mainMap;
            _mainMap.Disable();
        }

        //bool isUI = map == UIMap;
        //Cursor.lockState = isUI ? CursorLockMode.None : CursorLockMode.Locked;
        //Cursor.visible = isUI;

        _mainMap = map;
        _mainMap.Enable();
        OnMainMapSwitched?.Invoke(_mainMap);

        //Debug.Log($"Switched to action map: {_mainMap.name}");
    }

    public void ReturnToPreviousMode() {
        SwitchActionMap(previousMap);
    }

    public InputAction GetAction(string actionId) {
        return inputAsset.FindAction(actionId);
    }


}
