using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

public class AbilityUI : MonoBehaviour, IPointerEnterHandler, ITipProvider {
    [SerializeField] private TextMeshProUGUI abilityName;
    [SerializeField] private Image abilityIcon;

    private string description;

    private Canvas parentCanvas;
    [Inject] UIManager uiManager;

    private IObjectDistributer originPool;

    private void Start() {
        // Отримуємо батьківський Canvas
        parentCanvas = GetComponentInParent<Canvas>();
    }

    public void Initialize(IObjectDistributer distributor) {
        originPool = distributor;
    }

    public void CreateUISets(CardAbility cardAbility, bool abilityNamesEnabled = false) {
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
        uiManager.ShowTip(GetInfo());
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

    private void OnDestroy() {
        ResetUI();
        if (originPool != null) {
            originPool.ReleaseObject(gameObject); // Повернення в пул, якщо потрібно
        }
    }

    public string GetInfo() {
        return description;
    }
}
