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
        UpdateDescriptionContent(card);
    }

    private void UpdateDescriptionContent(Card card) {
        if (card is SpellCard spell) {
            // Show abilities or description
            List<CardAbility> cardAbilities = spell.AbilityManager.GetAbilities();

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
        
    }

    protected virtual void AttachmentToCard(Card card) {
        if (card == null) return;

        if (card is CreatureCard creatureCard) {
            UpdateHealth(creatureCard.Health.CurrentValue, creatureCard.Health.CurrentValue);
            UpdateAttack(creatureCard.Attack.CurrentValue, creatureCard.Attack.CurrentValue);
            

            creatureCard.Health.OnValueChanged += UpdateHealth;
            creatureCard.Attack.OnValueChanged += UpdateAttack;
        }

        UpdateCost(card.Cost.CurrentValue, card.Cost.CurrentValue);

        card.Cost.OnValueChanged += UpdateCost;
    }

    #region Updaters
    protected virtual void UpdateName(string newName) {
        if (nameText != null && !string.IsNullOrEmpty(newName)) {
            nameText.text = newName;
        } else {
            Debug.LogWarning("Attempted to set name to an empty or null value.");
        }
    }

    protected virtual void UpdateHealth(int beforeAmount, int currentAmount) {
        UpdateStat(healthText, beforeAmount, currentAmount);
    }

    protected virtual void UpdateAttack(int beforeAmount, int currentAmount) {
        UpdateStat(attackText, beforeAmount, currentAmount);
    }

    protected void UpdateCost(int beforeAmount, int currentAmount) {
        UpdateStat(costTMP, beforeAmount, currentAmount);
    }


    protected void UpdateStat(TMP_Text textComponent, int beforeAmount, int currentAmount) {
        if (textComponent != null) {
            textComponent.text = $"{currentAmount}";
        }
    }

    protected void UpdateAbilities(List<CardAbility> abilities) {
        foreach (var ability in abilities) {
            if (ability == null || ability.AbilityData == null) continue;

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
        if (cardAbilityPool == null) {
            cardAbilityPool = abilityUIPool;
        }
    }
}
