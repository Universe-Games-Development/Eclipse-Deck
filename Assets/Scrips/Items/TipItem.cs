using UnityEngine;
using Zenject;

public class TipItem : MonoBehaviour, ITipProvider {
    [SerializeField] private TipData tipData;

    [Inject] protected UIManager uiManager;

    public void Initialize(UIManager uiManager) {
        this.uiManager = uiManager;
    }

    void OnMouseEnter() {
        uiManager.ShowInfo(this);
    }

    void OnMouseExit() {
        uiManager.HideInfo(this);
    }

    public virtual string GetInfo() {
        return tipData != null ? tipData.tipText : "No tip available";
    }
}
