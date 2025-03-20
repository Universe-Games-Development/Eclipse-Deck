using System.Collections;
using TMPro;
using UnityEngine;
using Zenject;

public class UITimMonitor : MonoBehaviour {
    [Header("Settings")]
    [SerializeField] private float minDisplayDuration = 3f; // Мінімальний час показу в секундах
    [SerializeField] private float timePerCharacter = 0.1f; // Додатковий час за кожен символ

    [Header("References")]
    [SerializeField] private TextMeshProUGUI tipTextField;

    private UIManager uiManager;
    private string currentInfo;
    private Coroutine hideCoroutine;

    [Inject]
    public void Construct(UIManager uiManager) {
        this.uiManager = uiManager;
    }

    private void OnEnable() {
        //uiManager.OnInfoRequested += ShowInfo;
    }

    private void OnDisable() {
        //uiManager.OnInfoRequested -= ShowInfo;
    }

    public void ShowInfo(string info) {

        if (currentInfo != null && currentInfo.Equals(info)) {
            if (hideCoroutine != null) {
                StopCoroutine(hideCoroutine);
            }

            HideInfo();
        }

        currentInfo = info;
        tipTextField.text = info;

        float displayDuration = CalculateDisplayDuration(info);
        hideCoroutine = StartCoroutine(HideInfoAfterDelay(displayDuration));
    }

    public void HideInfo() {
        tipTextField.text = string.Empty;
        currentInfo = null;

        if (hideCoroutine != null) {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }
    }

    private IEnumerator HideInfoAfterDelay(float duration) {
        yield return new WaitForSeconds(duration);
        HideInfo();
    }

    private float CalculateDisplayDuration(string text) {
        if (string.IsNullOrEmpty(text)) {
            return minDisplayDuration;
        }
        return minDisplayDuration + (text.Length * timePerCharacter);
    }
}
