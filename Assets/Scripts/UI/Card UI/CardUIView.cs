using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class CardUIView : CardView, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {
    public Action<bool> OnCardHovered;
    public Action<bool, UniTask> OnCardSelection; // used by animator to lift card or lower
    public Func<CardUIView, UniTask> OnCardRemoval;
    public Action OnChanged;

    public RectTransform RectTransform;
    private CardLayoutGhost ghost;

    // Components
    [SerializeField] public CardAnimator DoTweenAnimator;
    
    public CardDescription Description;

    private void Awake() {
        if (RectTransform == null) {
            RectTransform = GetComponent<RectTransform>();
        }
        CardInfo.OnDataChanged += InvokeCardChangedEvent;
    }

    private void InvokeCardChangedEvent() {
        OnChanged?.Invoke();
    }

    private void OnDestroy() {
        if (CardInfo)
        CardInfo.OnDataChanged -= InvokeCardChangedEvent;
    }

    public void SetGhost(CardLayoutGhost ghost) {
        this.ghost = ghost;
    }

    public override void InitializeAnimator() {
        if (DoTweenAnimator == null) return;
        DoTweenAnimator.OnReachedLayout += () => SetInteractable(true);
    }

    public override void SetInteractable(bool value) {
        base.SetInteractable(value);
    }

    public override async UniTask RemoveCardView() {
        isInteractable = false;
        if (OnCardRemoval != null) {
            await OnCardRemoval.Invoke(this);
        }
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
        // Implement selection logic for UI cards
        DoTweenAnimator?.ToggleHover(true);
    }

    public override void Deselect() {
        // Implement deselection logic for UI cards
        DoTweenAnimator?.ToggleHover(false);
    }

    public void UpdatePosition() {
        if (ghost == null) return;

        Vector3 newLocalPosition = transform.parent.InverseTransformPoint(ghost.transform.position);

        SetInteractable(false);
        transform.DOLocalMove(newLocalPosition, 0.8f)
            .SetEase(Ease.InOutSine)
            .OnComplete(() => {
                SetInteractable(isInteractable);
            });
    }
}