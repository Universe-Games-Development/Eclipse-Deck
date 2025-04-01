using System;
using UnityEngine.InputSystem;

public class InputMapManager {
    public InputSystem_Actions inputAsset;

    public Action<InputActionMap> OnMapChanged;

    public InputActionMap enabledMap;

    public InputMapManager() {
        inputAsset = new InputSystem_Actions();
    }

    public void ToggleActionMap(InputActionMap mapToEnable) {
        if (mapToEnable.enabled) return;

        inputAsset.Disable();
        OnMapChanged?.Invoke(mapToEnable);
        mapToEnable.Enable();

        enabledMap = mapToEnable;
    }
}
