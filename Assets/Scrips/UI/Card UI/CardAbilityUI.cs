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

    public void FillAbilityUI(Ability ability, bool abilityNamesEnabled = false) {
        AbilityData abilityData = ability.AbilityData;

        if (ability == null || abilityData == null) {
            Debug.LogWarning("CardAbility or its data is null!");

            if (abilityName != null) abilityName.gameObject.SetActive(false);
            if (abilityIcon != null) abilityIcon.gameObject.SetActive(false);
        }

        description = abilityData.Description ?? string.Empty;

        if (abilityName != null) {
            if (abilityNamesEnabled && !string.IsNullOrEmpty(abilityData.Name)) {
                abilityName.text = abilityData.Name;
                abilityName.gameObject.SetActive(true);
            } else {
                abilityName.gameObject.SetActive(false);
            }
        }

        if (abilityIcon != null) {
            if (abilityData.Sprite != null) {
                abilityIcon.sprite = abilityData.Sprite;
                abilityIcon.gameObject.SetActive(true);
            } else {
                abilityIcon.gameObject.SetActive(false);
            }
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
