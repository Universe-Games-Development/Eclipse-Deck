using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardViewInfo : MonoBehaviour {
    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI costTMP;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private Image healthImage;
    [SerializeField] private TextMeshProUGUI attackText;
    [SerializeField] private Image attackImage;
    [SerializeField] private TextMeshProUGUI authorTMP;
    [SerializeField] private Image rarity;
    [SerializeField] private Image cardBackground;
    [SerializeField] private Image characterImage;

    public Action OnDataChanged;

    public void UpdateName(string newName) {
        if (nameText != null && !string.IsNullOrEmpty(newName)) {
            nameText.text = newName;
            OnDataChanged?.Invoke();
        } else {
            Debug.LogWarning("Attempted to set name to an empty or null value.");
        }
    }

    public void UpdateCost(int currentAmount) {
        if (costTMP != null) {
            costTMP.text = $"{currentAmount}";
            OnDataChanged?.Invoke();
        }
    }

    public void UpdateHealth(int currentAmount) {
        if (healthText != null) {
            healthText.text = $"{currentAmount}";
            OnDataChanged?.Invoke();
        }
    }

    public void UpdateAttack(int currentAmount) {
        if (attackText != null) {
            attackText.text = $"{currentAmount}";
            OnDataChanged?.Invoke();
        }
    }

    public void UpdateRarity(Color color) {
        rarity.color = color;
        OnDataChanged?.Invoke();
    }

    public void UpdateAuthor(string author) {
        authorTMP.text = author;
        OnDataChanged?.Invoke();
    }

    public void UpdatePortait(Sprite characterSprite) {
        characterImage.sprite = characterSprite;
        OnDataChanged?.Invoke();
    }

    public void UpdateBackground(Sprite characterSprite) {
        cardBackground.sprite = characterSprite;
        OnDataChanged?.Invoke();
    }

    public void TogglHealthText(bool isEnabled) {
        healthText?.gameObject.SetActive(isEnabled);
        healthImage?.gameObject.SetActive(isEnabled);
        OnDataChanged?.Invoke();
    }

    public void ToggleAttackText(bool isEnabled) {
        attackText?.gameObject.SetActive(isEnabled);
        attackImage?.gameObject.SetActive(isEnabled);
        OnDataChanged?.Invoke();
    }
}