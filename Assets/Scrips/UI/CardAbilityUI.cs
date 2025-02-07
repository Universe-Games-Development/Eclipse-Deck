using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

public class CardAbilityUI : MonoBehaviour, IPointerEnterHandler {
    [SerializeField] private TextMeshProUGUI abilityName;
    [SerializeField] private Image abilityIcon;

    private string description;

    [Inject] UIManager uiManager;

    public void FillAbilityUI(CardAbility cardAbility, bool abilityNamesEnabled = false) {
        if (cardAbility?.data != null) {
            // Оновити опис
            description = cardAbility.data.Description ?? string.Empty;

            // Установити назву
            if (abilityName != null) {
                if (abilityNamesEnabled && !string.IsNullOrEmpty(cardAbility.data.Name)) {
                    abilityName.text = cardAbility.data.Name;
                    abilityName.gameObject.SetActive(true);
                } else {
                    abilityName.gameObject.SetActive(false);
                }
            }

            // Установити іконку
            if (abilityIcon != null) {
                if (cardAbility.data.Sprite != null) {
                    abilityIcon.sprite = cardAbility.data.Sprite;
                    abilityIcon.gameObject.SetActive(true);
                } else {
                    abilityIcon.gameObject.SetActive(false);
                }
            }
        } else {
            Debug.LogWarning("CardAbility or its data is null!");

            // Деактивувати всі UI-елементи, якщо даних немає
            if (abilityName != null) abilityName.gameObject.SetActive(false);
            if (abilityIcon != null) abilityIcon.gameObject.SetActive(false);
        }
    }

    #region User interaction
    public void OnPointerEnter(PointerEventData eventData) {
        Debug.Log("Abiliti trigger on poiner enter");
    }
    #endregion

    public void ResetUI() {
        // Скинути назву
        if (abilityName != null) {
            abilityName.text = string.Empty;
            abilityName.gameObject.SetActive(true); // Включити текст назад, якщо він вимкнений
        }

        // Скинути іконку
        if (abilityIcon != null) {
            abilityIcon.sprite = null; // Видалити іконку
            abilityIcon.gameObject.SetActive(true);
        }
    }
}
