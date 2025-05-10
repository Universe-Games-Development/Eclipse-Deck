using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardUIInfo : MonoBehaviour {
    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI costTMP;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI attackText;
    [SerializeField] private TextMeshProUGUI authorTMP;
    [SerializeField] private Image rarity;
    [SerializeField] private Image cardBackground;
    [SerializeField] private Image characterImage;

    public Action OnDataChanged;

    private bool _isBatchUpdating = false;
    private bool _wasUpdated = false;

    // Общий метод для уведомления об изменениях
    private void NotifyDataChanged() {
        if (!_isBatchUpdating) {
            OnDataChanged?.Invoke();
        } else {
            _wasUpdated = true;
        }
    }

    public void UpdateName(string newName) {
        if (nameText != null && !string.IsNullOrEmpty(newName)) {
            nameText.text = newName;
            NotifyDataChanged();
        } else {
            Debug.LogWarning("Attempted to set name to an empty or null value.");
        }
    }

    public void UpdateCost(int currentAmount) {
        if (costTMP != null) {
            costTMP.text = $"{currentAmount}";
            NotifyDataChanged();
        }
    }

    public void UpdateHealth(int currentAmount) {
        if (healthText != null) {
            healthText.text = $"{currentAmount}";
            NotifyDataChanged();
        }
    }

    public void UpdateAttack(int currentAmount) {
        if (attackText != null) {
            attackText.text = $"{currentAmount}";
            NotifyDataChanged();
        }
    }

    public void UpdateRarity(Color color) {
        rarity.color = color;
        NotifyDataChanged();
    }

    public void UpdateAuthor(string author) {
        authorTMP.text = author;
        NotifyDataChanged();
    }

    public void UpdateCharacterImage(Sprite characterSprite) {
        characterImage.sprite = characterSprite;
        NotifyDataChanged();
    }

    // Метод для группового обновления свойств карты
    public void BatchUpdate(Action<CardUIInfo> updateActions) {
        if (updateActions == null) return;

        _isBatchUpdating = true;
        _wasUpdated = false;

        updateActions.Invoke(this);

        _isBatchUpdating = false;

        // Вызываем событие только если действительно что-то изменилось
        if (_wasUpdated) {
            OnDataChanged?.Invoke();
        }
    }

    // Удобный метод для обновления нескольких свойств
    public void UpdateCard(string name = null, int? cost = null, int? health = null,
                          int? attack = null, Color? rarityColor = null,
                          string author = null, Sprite characterSprite = null) {
        BatchUpdate(card => {
            if (name != null) card.UpdateName(name);
            if (cost.HasValue) card.UpdateCost(cost.Value);
            if (health.HasValue) card.UpdateHealth(health.Value);
            if (attack.HasValue) card.UpdateAttack(attack.Value);
            if (rarityColor.HasValue) card.UpdateRarity(rarityColor.Value);
            if (author != null) card.UpdateAuthor(author);
            if (characterSprite != null) card.UpdateCharacterImage(characterSprite);
        });
    }
}