using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardUIInfo : MonoBehaviour
{
    protected Card card;
    public string Id => id;
    [Header("Params")]
    [SerializeField] private string id;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI costTMP;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI attackText;
    [SerializeField] private Image rarity;

    [Header("Visuals")]
    [SerializeField] private TextMeshProUGUI authorTMP;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image cardBackground;
    [SerializeField] private Image characterImage;

    [SerializeField] private RectTransform abilityFiller;
    List<CardAbilityUI> abilityUIs = new();

    private CardAbilityPool cardAbilityPool;
    public void FillData(Card card) {
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

        // Show abilities or description
        List<CardAbility> cardAbilities = card.cardAbilities;

        if (cardAbilities == null || cardAbilities.Count == 0) {
            EnableCardDescription();
        } else {
            // Hide description if abilities exist
            if (descriptionText != null) {
                descriptionText.gameObject.SetActive(false);
            }

            UpdateAbilities(cardAbilities);
        }
    }

    protected virtual void AttachmentToCard(Card card) {
        if (card == null) return;

        if (card is CreatureCard creatureCard) {
            UpdateHealth(creatureCard.HealthStat?.CurrentValue ?? 0, creatureCard.HealthStat?.CurrentValue ?? 0);
            UpdateAttack(creatureCard.Attack?.CurrentValue ?? 0, creatureCard.Attack?.CurrentValue ?? 0);
            UpdateCost(creatureCard.Cost?.CurrentValue ?? 0, creatureCard.Cost?.CurrentValue ?? 0);

            creatureCard.HealthStat.OnValueChanged += UpdateHealth;
            creatureCard.Attack.OnValueChanged += UpdateAttack;
            creatureCard.Cost.OnValueChanged += UpdateCost;
        }
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

    protected void UpdateAbilities(List<CardAbility> abilities) {
        foreach (var ability in abilities) {
            if (ability == null || ability.abilityData == null) continue;

            CardAbilityUI abilityUI = cardAbilityPool.Get();
            abilityUI.transform.SetParent(abilityFiller);

            abilityUI.FillAbilityUI(ability, true);
            abilityUIs.Add(abilityUI);
        }
    }
    #endregion

    private void EnableCardDescription() {
        if (descriptionText != null && card != null) {
            descriptionText.text = card.Data.Description;
            descriptionText.gameObject.SetActive(true);
        }
    }

    public void CleanAbilities() {

    }

    public void Reset() {
        CleanAbilities();
        card = null;
    }

    public void SetAbilityFactory(CardAbilityPool abilityUIPool) {
        if (cardAbilityPool != null) {
            cardAbilityPool = abilityUIPool;
        }
    }
}
