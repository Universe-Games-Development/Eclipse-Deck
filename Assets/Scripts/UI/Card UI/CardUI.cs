using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {
    public Action<bool> OnCardHovered;
    public Action<CardUI> OnCardClicked; // used by cardHand to define selected card
    public Action<bool, UniTask> OnCardSelection; // used by animator to lift card or lower
    public Func<CardUI, UniTask> OnCardRemoval;
    public Action OnLayoutUpdate;

    private bool isInteractable;
    
    // Components
    [SerializeField] public CardAnimator DoTweenAnimator;
    [SerializeField] public CardUIInfo UIDataInfo;

    private void Awake() {
        InitializeAnimator();
    }

    public void InitializeAnimator() {
        DoTweenAnimator.AttachAnimator(this);
        DoTweenAnimator.OnReachedLayout += () => SetInteractable(true);
    }

    public void SetInteractable(bool value) => isInteractable = value;

    public void SetAbilityPool(CardAbilityPool AbilityUIPool) {
        if (AbilityUIPool != null) {
            UIDataInfo.SetAbilityFactory(AbilityUIPool);
        }
    }

    public void SetCardLogic(Card card) {
        UIDataInfo.FillData(card);
    }

    public async UniTask RemoveCardUI() {
        isInteractable = false;
        if (OnCardRemoval != null) {
            await OnCardRemoval.Invoke(this);
        }
        await UniTask.CompletedTask;
        if (DoTweenAnimator.CardLayoutGhost != null) {
            Destroy(DoTweenAnimator.CardLayoutGhost.gameObject);
        }
        Destroy(gameObject);
    }


    public void OnPointerEnter(PointerEventData eventData) {
        if (!isInteractable) return;
        OnCardHovered?.Invoke(true);
    }

    public void OnPointerExit(PointerEventData eventData) {
        if (!isInteractable) return;
        OnCardHovered?.Invoke(false);
    }
    public void OnPointerClick(PointerEventData eventData) {
        if (!isInteractable) return;
        OnCardClicked?.Invoke(this);
    }

    public void Reset() {
        DoTweenAnimator?.Reset();
        UIDataInfo?.Reset();
        isInteractable = false;
    }

    internal void UpdateLayout() {
        OnLayoutUpdate?.Invoke();
    }
}
