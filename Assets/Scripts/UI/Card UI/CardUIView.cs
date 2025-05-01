using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public interface ICardView {
    public string Id { get; set; }
    void InitializeAnimator();
    void OnPointerClick(PointerEventData eventData);
    void OnPointerEnter(PointerEventData eventData);
    void OnPointerExit(PointerEventData eventData);
    UniTask RemoveCardView();
    void Reset();
    void SetCardData(CardData cardData);
    void SetInteractable(bool value);
    void UpdateCost(int from, int to);
    void UpdateHealth(int from, int to);
}

public class CardUIView : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, ICardView {
    public Action<bool> OnCardHovered;
    public Action<CardUIView> OnCardClicked; // used by cardHand to define selected card
    public Action<bool, UniTask> OnCardSelection; // used by animator to lift card or lower
    public Func<CardUIView, UniTask> OnCardRemoval;

    private bool isInteractable;
    private CardLayoutGhost ghost;
    

    [Header("Params")]
    public string Id { get; set; }
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI costTMP;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI attackText;
    [SerializeField] private Image rarity;

    [Header("Visuals")]
    [SerializeField] private TextMeshProUGUI authorTMP;

    [SerializeField] private Image cardBackground;
    [SerializeField] private Image characterImage;

    // Components
    [SerializeField] public CardAnimator DoTweenAnimator;
    [SerializeField] public CardUIInfo UIDataInfo;
    public CardDescription Description;

    private void Awake() {
        InitializeAnimator();
    }

    public void SetCardData(CardData data) {
        if (data == null) {
            Debug.LogError("data is null during initialization!");
            return;
        }

        // Visuals
        
        
       
    }

    public void SetGhost(CardLayoutGhost ghost) {
        this.ghost = ghost;
    }

    #region Updaters

    public void UpdateRarity(Color color) {
        rarity.color = color;
    }

    public void UpdateAuthor(string author) {
        authorTMP.text = author;
    }

    public void UpdateAuthor(Sprite characterSprite) {
        characterImage.sprite = characterSprite;
    }

    // Base Card UI
    public void UpdateName(string newName) {
        if (nameText != null && !string.IsNullOrEmpty(newName)) {
            nameText.text = newName;
        } else {
            Debug.LogWarning("Attempted to set name to an empty or null value.");
        }
    }

    public void UpdateCost(int beforeAmount, int currentAmount) {
        UpdateStat(costTMP, beforeAmount, currentAmount);
    }

    // CreatureCard UI
    public void UpdateHealth(int beforeAmount, int currentAmount) {
        UpdateStat(healthText, beforeAmount, currentAmount);
    }

    public void UpdateAttack(int beforeAmount, int currentAmount) {
        UpdateStat(attackText, beforeAmount, currentAmount);
    }

    protected void UpdateStat(TMP_Text textComponent, int beforeAmount, int currentAmount) {
        if (textComponent != null) {
            textComponent.text = $"{currentAmount}";
        }
    }


    #endregion
    public void InitializeAnimator() {
        if (DoTweenAnimator == null) return;
        DoTweenAnimator.AttachAnimator(this);
        DoTweenAnimator.OnReachedLayout += () => SetInteractable(true);
    }

    public void SetInteractable(bool value) => isInteractable = value;



    public async UniTask RemoveCardView() {
        isInteractable = false;
        if (OnCardRemoval != null) {
            await OnCardRemoval.Invoke(this);
        }
        await UniTask.CompletedTask;
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
        isInteractable = false;
    }

    internal void SetAbilityPool(CardAbilityPool abilityPool) {
        throw new NotImplementedException();
    }

    internal void UpdatePosition() {
        Vector3 newLocalPosition = transform.parent.InverseTransformPoint(ghost.transform.position);

        SetInteractable(false);
        transform.DOLocalMove(newLocalPosition, 0.8f)
            .SetEase(Ease.InOutSine)
            .OnComplete(() => {
                {
                    SetInteractable(isInteractable);
                }
            });
    }
}
