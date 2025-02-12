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
        if (cardAbility?.abilityData != null) {
            // ������� ����
            description = cardAbility.abilityData.Description ?? string.Empty;

            // ���������� �����
            if (abilityName != null) {
                if (abilityNamesEnabled && !string.IsNullOrEmpty(cardAbility.abilityData.Name)) {
                    abilityName.text = cardAbility.abilityData.Name;
                    abilityName.gameObject.SetActive(true);
                } else {
                    abilityName.gameObject.SetActive(false);
                }
            }

            // ���������� ������
            if (abilityIcon != null) {
                if (cardAbility.abilityData.Sprite != null) {
                    abilityIcon.sprite = cardAbility.abilityData.Sprite;
                    abilityIcon.gameObject.SetActive(true);
                } else {
                    abilityIcon.gameObject.SetActive(false);
                }
            }
        } else {
            Debug.LogWarning("CardAbility or its data is null!");

            // ������������ �� UI-��������, ���� ����� ����
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
        // ������� �����
        if (abilityName != null) {
            abilityName.text = string.Empty;
            abilityName.gameObject.SetActive(true); // �������� ����� �����, ���� �� ���������
        }

        // ������� ������
        if (abilityIcon != null) {
            abilityIcon.sprite = null; // �������� ������
            abilityIcon.gameObject.SetActive(true);
        }
    }
}
