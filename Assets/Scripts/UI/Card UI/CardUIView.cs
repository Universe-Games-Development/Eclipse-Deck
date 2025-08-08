using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class CardUIView : CardView, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {
    public Action<bool> OnCardHovered;
    public Action<bool, UniTask> OnCardSelection;
    public Func<CardUIView, UniTask> OnCardRemoval;
    public Action OnChanged;

    public RectTransform RectTransform;
    [SerializeField] public CardAnimator DoTweenAnimator;
    public CardDescription Description;

    protected override void Awake() {
        base.Awake();
        RectTransform ??= GetComponent<RectTransform>();
        CardInfo.OnDataChanged += InvokeCardChangedEvent;
    }

    protected override void OnDestroy() {
        if (CardInfo)
            CardInfo.OnDataChanged -= InvokeCardChangedEvent;
    }

    private void InvokeCardChangedEvent() => OnChanged?.Invoke();

    public override void SetInteractable(bool value) {
        base.SetInteractable(value);
    }

    public override async UniTask RemoveCardView() {
        isInteractable = false;
        if (OnCardRemoval != null)
            await OnCardRemoval.Invoke(this);
        await base.RemoveCardView();
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if (!isInteractable) return;
        DoTweenAnimator.ToggleHover(true);
        OnCardHovered?.Invoke(true);
    }

    public void OnPointerExit(PointerEventData eventData) {
        if (!isInteractable) return;
        DoTweenAnimator.ToggleHover(false);
        OnCardHovered?.Invoke(false);
    }

    public void OnPointerClick(PointerEventData eventData) {
        if (!isInteractable) return;
        DoTweenAnimator.ShrinkClick();
        RaiseCardClickedEvent();
    }

    public override void Reset() {
        DoTweenAnimator?.Reset();
        base.Reset();
    }

    public override void Select() {
        DoTweenAnimator?.ToggleHover(true);
    }

    public override void Deselect() {
        DoTweenAnimator?.ToggleHover(false);
    }
}
