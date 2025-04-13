using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardPresenter {
    public Card Card;
    public CardView View;
    public CardPresenter(Card card, CardView cardView) {
        Card = card;
        View = cardView;

        cardView.SetCardData(card.Data);
        AttachmentToCard(card);
    }
    private void AttachmentToCard(Card card) {
        if (card is CreatureCard creatureCard) {
            View.UpdateHealth(creatureCard.Health.CurrentValue, creatureCard.Health.CurrentValue);
            View.UpdateAttack(creatureCard.Attack.CurrentValue, creatureCard.Attack.CurrentValue);


            creatureCard.Health.OnValueChanged += UpdateHealth;
            creatureCard.Attack.OnValueChanged += UpdateAttack;
        }

        View.UpdateCost(card.Cost.CurrentValue, card.Cost.CurrentValue);

        card.Cost.OnValueChanged += UpdateCost;
    }

    private void UpdateCost(int from, int to) {
        View.UpdateCost(from, to);
    }

    private void UpdateAttack(int from, int to) {
        View.UpdateHealth(from, to);
    }

    private void UpdateHealth(int from, int to) {
        View.UpdateHealth(from, to);
    }

    private void UpdateDescriptionContent(Card card) {
        List<Ability<CardAbilityData, Card>> cardAbilities = card._abilityManager.GetAbilities();

        if (cardAbilities == null || cardAbilities.Count == 0) {
            View.Description.SetDescripton(card.Data.Description);
        } else {
            View.Description.UpdateAbilities(cardAbilities);
        }
    }
}

public class CardView : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, ICardView {
    public Action<bool> OnCardHovered;
    public Action<CardView> OnCardClicked; // used by cardHand to define selected card
    public Action<bool, UniTask> OnCardSelection; // used by animator to lift card or lower
    public Func<CardView, UniTask> OnCardRemoval;

    private bool isInteractable;

    [Header("Params")]
    [SerializeField] private string id;
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
        rarity.color = data.GetRarityColor();
        authorTMP.text = data.AuthorName;
        characterImage.sprite = data.CharacterSprite;
    }


    #region Updaters
    // CreatureCard UI
    public void UpdateHealth(int beforeAmount, int currentAmount) {
        UpdateStat(healthText, beforeAmount, currentAmount);
    }

    public void UpdateAttack(int beforeAmount, int currentAmount) {
        UpdateStat(attackText, beforeAmount, currentAmount);
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

    protected void UpdateStat(TMP_Text textComponent, int beforeAmount, int currentAmount) {
        if (textComponent != null) {
            textComponent.text = $"{currentAmount}";
        }
    }
    
    
    #endregion
    public void InitializeAnimator() {
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
}

public class CardDescription : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private RectTransform abilityFiller;
    private CardAbilityPool cardAbilityPool;
    List<CardAbilityUI> abilityUIs = new();

    public void SetDescripton(string description) {
        if (descriptionText != null) {
            descriptionText.gameObject.SetActive(true);
            descriptionText.text = description;
        }
    }

    public void UpdateAbilities(List<Ability<CardAbilityData, Card>> cardAbilities) {
        if (descriptionText != null) {
            descriptionText.gameObject.SetActive(false);
        }
        foreach (var ability in cardAbilities) {
            if (ability == null || ability.Data == null) continue;

            CardAbilityUI abilityUI = cardAbilityPool.Get();
            abilityUI.transform.SetParent(abilityFiller);

            abilityUI.FillAbilityUI(ability, true);
            abilityUIs.Add(abilityUI);
        }
    }

    internal void SetAbilityPool(CardAbilityPool abilityPool) {
        throw new NotImplementedException();
    }
}