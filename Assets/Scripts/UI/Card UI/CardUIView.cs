
using System;
using UnityEngine;

public class CardUIView : CardView {
    public Action OnChanged;

    public RectTransform RectTransform;
    [SerializeField] public CardAnimator DoTweenAnimator;
    public CardDescription Description;
    public CardViewInfo uiInfo;

    protected override void Awake() {
        base.Awake();
        RectTransform ??= GetComponent<RectTransform>();
        if (uiInfo == null) uiInfo = GetComponent<CardViewInfo>();
        uiInfo.OnDataChanged += InvokeCardChangedEvent;
    }

    protected override void OnDestroy() {
        if (uiInfo)
            uiInfo.OnDataChanged -= InvokeCardChangedEvent;
    }

    private void InvokeCardChangedEvent() => OnChanged?.Invoke();



    public void Reset() {
        DoTweenAnimator?.Reset();
    }

    public override void SetRenderOrder(int sortingOrder) {
        throw new NotImplementedException();
    }

    public override void ModifyRenderOrder(int modifyValue) {
        throw new NotImplementedException();
    }

    public override void ResetRenderOrder() {
        throw new NotImplementedException();
    }

    public override void SetHoverState(bool isHovered) {
        throw new NotImplementedException();
    }

    public override void UpdateDisplay(CardDisplayContext context) {
        throw new NotImplementedException();
    }
}
