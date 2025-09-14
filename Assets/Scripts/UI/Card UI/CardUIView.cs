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


    public void OnPointerEnter(PointerEventData eventData) {
        HandleMouseEnter();
    }

    public void OnPointerExit(PointerEventData eventData) {
        HandleMouseExit();
    }

    public void OnPointerClick(PointerEventData eventData) {
        HandleMouseDown();
    }

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

    #region UI Info Update
    public override void UpdateCost(int cost) {
        uiInfo.UpdateCost(cost);
    }

    public override void UpdateName(string name) {
        uiInfo.UpdateName(name);
    }

    public override void UpdateAttack(int attack) {
        uiInfo.UpdateAttack(attack);
    }

    public override void UpdateHealth(int health) {
        uiInfo.UpdateHealth(health);
    }

    public override void ToggleCreatureStats(bool isEnabled) {
        uiInfo.ToggleAttackText(isEnabled);
        uiInfo.TogglHealthText(isEnabled);
    }

    public override void UpdatePortait(Sprite portait) {
        uiInfo.UpdatePortait(portait);
    }

    public override void UpdateBackground(Sprite bgImage) {
        uiInfo.UpdateBackground(bgImage);
    }

    public override void UpdateRarity(Color rarity) {
        uiInfo.UpdateRarity(rarity);
    }
    #endregion
}
