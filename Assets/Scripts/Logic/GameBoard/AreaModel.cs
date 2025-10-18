using System;
using UnityEngine;

/// <summary>
/// Unit that can be placed in a cell area (spawnZone player castle etc.)
/// </summary>
public class AreaModel : UnitModel {
    public event Action<AreaModel> OnSizeChanged;
    public Vector3 CurrentSize;

    public void SetSize(Vector3 newSize) {
        if (newSize == null) throw new ArgumentNullException(nameof(newSize));
        CurrentSize = newSize;
        OnSizeChanged?.Invoke(this);
    }
}
