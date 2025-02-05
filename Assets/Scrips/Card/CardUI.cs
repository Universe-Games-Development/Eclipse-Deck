using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {
    public Action<CardUI> OnCardExit;
    public Action<CardUI> OnCardEntered;
    public Action<CardUI> OnCardClicked;

    [Header("Params")]
    [SerializeField] private string id;
    [SerializeField] private Image rarity;
    [SerializeField] private TextMeshProUGUI costTMP;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI attackText;
    [SerializeField] private TextMeshProUGUI nameText;

    [Header("Visuals")]
    [SerializeField] private TextMeshProUGUI authorTMP;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image cardBackground;
    [SerializeField] private Image characterImage;

    [Header("Layout")]
    [SerializeField] private RectTransform innerBody;
    [SerializeField] private ObjectDistributer abilityUIDistributer;
    [SerializeField] private RectTransform abilityFiller;

    protected List<AbilityUI> abilityUIs = new List<AbilityUI>();
    protected Card card;
    private ObjectDistributer pool;
    private CardAnimator animator;
    private bool isInteractable;

    public string Id => id;

    private void SetInteractable(bool value) => isInteractable = value;

    public void SetPool(ObjectDistributer pool) => this.pool = pool;

    public async UniTask RemoveCardController() {
        if (animator != null) {
            await animator.FlyAwayWithCallback();
        }
        pool?.ReleaseObject(gameObject);
    }

    public void Initialize(Card card) {
        if (card == null) {
            Debug.LogError("Card is null during initialization!");
            return;
        }

        this.card = card;

        // Visuals
        rarity.color = card.Data.GetRarityColor();
        authorTMP.text = card.Data.AuthorName;
        characterImage.sprite = card.Data.CharacterSprite;

        // Logic
        AttachmentToCard(card);

        // Show abilities
        List<CardAbility> cardAbilities = card.cardAbilities;

        if (cardAbilities == null || cardAbilities.Count == 0) {
            EnableCardDescription();
            return;
        }

        // Hide description if abilities exist
        if (descriptionText != null) {
            descriptionText.gameObject.SetActive(false);
        }

        UpdateAbilities(cardAbilities);
    }

    protected virtual void AttachmentToCard(Card card) {
        if (card == null) return;

        if (card is CreatureCard creatureCard) {
            UpdateHealth(creatureCard.Health?.CurrentValue ?? 0, creatureCard.Health?.CurrentValue ?? 0);
            UpdateAttack(creatureCard.Attack?.CurrentValue ?? 0, creatureCard.Attack?.CurrentValue ?? 0);
            UpdateCost(creatureCard.Cost?.CurrentValue ?? 0, creatureCard.Cost?.CurrentValue ?? 0);

            creatureCard.Health.OnValueChanged += UpdateHealth;
            creatureCard.Attack.OnValueChanged += UpdateAttack;
            creatureCard.Cost.OnValueChanged += UpdateCost;
        }
    }

    public void InitializeAnimator(Vector3 initialPosition) {
        transform.position = initialPosition;
        animator?.FlyToOrigin();
        animator.OnReachedOrigin += () => SetInteractable(true);
    }

    #region Updaters
    protected virtual void UpdateName(string newName) {
        if (nameText != null && !string.IsNullOrEmpty(newName)) {
            nameText.text = newName;
        } else {
            Debug.LogWarning("Attempted to set name to an empty or null value.");
        }
    }

    protected virtual void UpdateHealth(int currentHealth, int initialHealth) {
        if (healthText != null) {
            healthText.text = currentHealth > 0 ? $"{currentHealth}" : "0";
        }
    }

    protected virtual void UpdateAttack(int currentAttack, int initialAttack) {
        if (attackText != null) {
            attackText.text = currentAttack >= 0 ? $"{currentAttack}" : "0";
        }
    }

    protected void UpdateCost(int currentCost, int initialCost) {
        if (costTMP != null) {
            costTMP.text = currentCost >= 0 ? $"{initialCost}" : "0";
        }
    }
    #endregion

    protected void UpdateAbilities(List<CardAbility> abilities) {
        foreach (var ability in abilities) {
            if (ability == null || ability.data == null) continue;

            GameObject newAbilityObj = abilityUIDistributer.CreateObject();
            if (newAbilityObj == null) continue;

            if (!newAbilityObj.TryGetComponent(out AbilityUI abilityUI)) {
                Debug.LogWarning("Created object does not have an AbilityUI component.");
                Destroy(newAbilityObj);
                continue;
            }

            abilityUI.CreateUISets(ability, true);
            abilityUIs.Add(abilityUI);
        }
    }

    private void EnableCardDescription() {
        if (descriptionText != null && card != null) {
            descriptionText.text = card.Data.Description;
            descriptionText.gameObject.SetActive(true);
        }
    }

    public void HandleSelection() => characterImage.color = Color.green;

    public void HandleDeselection() => characterImage.color = Color.white;

    public void OnPointerEnter(PointerEventData eventData) {
        animator?.ToggleHover(true);
        OnCardEntered?.Invoke(this);
    }

    public void OnPointerExit(PointerEventData eventData) {
        animator?.ToggleHover(false);
        OnCardExit?.Invoke(this);
    }

    public void OnPointerClick(PointerEventData eventData) => OnCardClicked?.Invoke(this);

    public void Reset() {
        animator?.Reset();
        isInteractable = false;
        card = null;
    }
}
