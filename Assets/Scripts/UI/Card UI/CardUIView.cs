using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardUIView : CardView, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {
    public Action OnChanged;

    public RectTransform RectTransform;
    [SerializeField] public CardAnimator DoTweenAnimator;
    public CardDescription Description;

    protected override void Awake() {
        base.Awake();
        RectTransform ??= GetComponent<RectTransform>();
        if (uiInfo == null) uiInfo = GetComponent<CardUIInfo>();
        uiInfo.OnDataChanged += InvokeCardChangedEvent;
    }

    protected override void OnDestroy() {
        if (uiInfo)
            uiInfo.OnDataChanged -= InvokeCardChangedEvent;
    }

    private void InvokeCardChangedEvent() => OnChanged?.Invoke();


    public void OnPointerEnter(PointerEventData eventData) {
        HandleMouseEnter();
    }

    public void OnPointerExit(PointerEventData eventData) {
        HandleMouseExit();
    }

    public void OnPointerClick(PointerEventData eventData) {
        HandleMouseDown();
    }

    public override void Reset() {
        DoTweenAnimator?.Reset();
        base.Reset();
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
}
