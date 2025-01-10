using UnityEngine;
using Zenject;

public class TurnButton : MonoBehaviour, ITipProvider {
    [Inject] protected UIManager uiManager;
    [SerializeField] private TipDataSO tipData;

    void OnMouseEnter() {
        uiManager.ShowTip(this);
    }

    public virtual string GetInfo() {
        return tipData != null ? tipData.tipText : "No tip available";
    }

    private void OnMouseUpAsButton() {
        Debug.Log("Turn changed!");
    }
}
