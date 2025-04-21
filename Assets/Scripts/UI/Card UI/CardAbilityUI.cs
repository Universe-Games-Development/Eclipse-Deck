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

    public void FillAbilityUI(bool abilityNamesEnabled = false) {


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
