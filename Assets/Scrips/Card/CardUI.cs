using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {
    public Action<CardUI> OnCardExit;
    public Action<CardUI> OnCardEntered;
    public Action<CardUI> OnCardClicked;
    
    private bool isInteractable;
    
    // Components
    [SerializeField] public CardAnimator _doAnimator;
    [SerializeField] public CardUIInfo UIDataInfo;
    private UICardFactory uICardFactory;

    

    private void Awake() {
        InitializeAnimator();
    }
    public void InitializeAnimator() {
        _doAnimator.AttachAnimator(this);
        _doAnimator.OnReachedLayout += () => SetInteractable(true);
    }

    private void SetInteractable(bool value) => isInteractable = value;

    public void SetCardFactory(UICardFactory uICardFactory) {
        if (uICardFactory == null) {
            this.uICardFactory = uICardFactory;
            UIDataInfo.SetAbilityFactory(uICardFactory.AbilityUIFactory);
        }
    }

    public async UniTask RemoveCardUI() {
        if (_doAnimator != null) {
            await _doAnimator.FlyAwayWithCallback();
        }
        uICardFactory?.ReleaseCardUI(this);
    }


    public void OnPointerEnter(PointerEventData eventData) {
        _doAnimator?.ToggleHover(true);
        OnCardEntered?.Invoke(this);
    }

    public void OnPointerExit(PointerEventData eventData) {
        _doAnimator?.ToggleHover(false);
        OnCardExit?.Invoke(this);
    }

    public void OnPointerClick(PointerEventData eventData) => OnCardClicked?.Invoke(this);

    public void Reset() {
        _doAnimator?.Reset();
        UIDataInfo?.Reset();
        isInteractable = false;
    }

    internal void UpdateLayout() {
        _doAnimator.FlyByLayout();
    }

    internal void HandleSelection() {
        Debug.Log("Do something selected card");
    }

    internal void HandleDeselection() {
        Debug.Log("Do something deselected card");
    }

    public void Initialize(Card card) {
        UIDataInfo.FillData(card);
    }
}
