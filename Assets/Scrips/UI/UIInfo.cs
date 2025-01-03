using TMPro;
using UnityEngine;
using System.Collections;

public class UIInfo : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI m_TextMeshProUGUI;
    [SerializeField] private float minDisplayDuration = 3f; // ̳�������� ��� ������ � ��������
    [SerializeField] private float timePerCharacter = 0.1f; // ���������� ��� �� ����� ������

    private ITipProvider currentTipProvider;
    private Coroutine hideCoroutine;

    public void ShowInfo(ITipProvider tipProvider) {
        if (tipProvider == null) return;

        if (hideCoroutine != null) {
            StopCoroutine(hideCoroutine);
        }

        currentTipProvider = tipProvider;
        m_TextMeshProUGUI.text = tipProvider.GetInfo();

        // ���������� ��������� ������
        float displayDuration = CalculateDisplayDuration(tipProvider.GetInfo());

        // ��������� �������� ��� ������������� ������������ ����������
        hideCoroutine = StartCoroutine(HideInfoAfterDelay(displayDuration));
    }

    private IEnumerator HideInfoAfterDelay(float duration) {
        yield return new WaitForSeconds(duration);

        HideInfo(currentTipProvider);
    }

    public void HideInfo(ITipProvider tipProvider) {
        if (tipProvider != currentTipProvider) return;

        m_TextMeshProUGUI.text = string.Empty;
        currentTipProvider = null;

        if (hideCoroutine != null) {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }
    }

    private float CalculateDisplayDuration(string text) {
        if (string.IsNullOrEmpty(text)) return minDisplayDuration;

        // ����������: ��������� ��� + ��� �� ����� ������
        float calculatedDuration = minDisplayDuration + (text.Length * timePerCharacter);
        return calculatedDuration;
    }
}
