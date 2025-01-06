using System.Collections;
using TMPro;
using UnityEngine;
using Zenject;

public class UIInfo : MonoBehaviour {
    [Header("Settings")]
    [SerializeField] private float minDisplayDuration = 3f; // ̳�������� ��� ������ � ��������
    [SerializeField] private float timePerCharacter = 0.1f; // ���������� ��� �� ����� ������

    [Header("References")]
    [SerializeField] private TextMeshProUGUI tipTextField;

    private UIManager uiManager;
    private ITipProvider currentTipProvider;
    private Coroutine hideCoroutine;

    [Inject]
    public void Construct(UIManager uiManager) {
        this.uiManager = uiManager;
    }

    private void OnEnable() {
        uiManager.OnInfoItemEnter += ShowInfo;
        uiManager.OnInfoItemExit += HideInfo;
    }

    private void OnDisable() {
        uiManager.OnInfoItemEnter -= ShowInfo;
        uiManager.OnInfoItemExit -= HideInfo;
    }

    public void ShowInfo(ITipProvider tipProvider) {
        if (tipProvider == null) return;

        // ��������� ��������� ��������, ���� ���� ����
        if (hideCoroutine != null) {
            StopCoroutine(hideCoroutine);
        }

        // �������� ��������� ������������� �� ���������� �����
        currentTipProvider = tipProvider;
        tipTextField.text = tipProvider.GetInfo();

        // ���������� ��������� ������ �� ��������� �������� ��� ������������� ������������
        float displayDuration = CalculateDisplayDuration(tipProvider.GetInfo());
        hideCoroutine = StartCoroutine(HideInfoAfterDelay(displayDuration));
    }

    public void HideInfo(ITipProvider tipProvider) {
        if (tipProvider != currentTipProvider) return;

        // ������� ����� � ������� ����
        tipTextField.text = string.Empty;
        currentTipProvider = null;

        // ��������� ��������, ���� ���� ����
        if (hideCoroutine != null) {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }
    }

    private IEnumerator HideInfoAfterDelay(float duration) {
        yield return new WaitForSeconds(duration);
        HideInfo(currentTipProvider);
    }

    private float CalculateDisplayDuration(string text) {
        if (string.IsNullOrEmpty(text)) {
            return minDisplayDuration;
        }
        return minDisplayDuration + (text.Length * timePerCharacter);
    }
}
