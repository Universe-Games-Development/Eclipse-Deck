using System;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public Action<ITipProvider> OnInfoItemEnter;
    public Action<ITipProvider> OnInfoItemExit;

    internal void HideInfo(ITipProvider tipItem) {
        OnInfoItemExit.Invoke(tipItem);
    }

    internal void ShowInfo(ITipProvider tipItem) {
        OnInfoItemEnter?.Invoke(tipItem);
    }
}
