using UnityEngine;
using Zenject;

public class TipItem : MonoBehaviour, ITipProvider {
    [SerializeField] private TipData tipData;

    protected UIInfo uiInfo;

    [Inject]
    private void Construct(UIInfo uiInfo) {
        this.uiInfo = uiInfo;
    }

    public void InitializeUIInfo(UIInfo uiInfo) {
        if (!this.uiInfo) {
            this.uiInfo = uiInfo;
        }
    }

    void OnMouseEnter() {
        uiInfo.ShowInfo(this);
    }

    void OnMouseExit() {
        uiInfo.HideInfo(this);
    }

    public virtual string GetInfo() {
        return tipData != null ? tipData.tipText : "No tip available";
    }
}
