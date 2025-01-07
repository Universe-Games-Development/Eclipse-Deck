using System.Collections;
using TMPro;
using UnityEngine;
using Zenject;

public class UIInfo : MonoBehaviour {
    [Header("Settings")]
    [SerializeField] private float minDisplayDuration = 3f; // Мінімальний час показу в секундах
    [SerializeField] private float timePerCharacter = 0.1f; // Додатковий час за кожен символ

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
    }

    private void OnDisable() {
        uiManager.OnInfoItemEnter -= ShowInfo;
    }

    public void ShowInfo(ITipProvider tipProvider) {
        if (tipProvider == null) return;

        // Якщо поточний постачальник не такий, як новий — ховаємо попередній і показуємо новий
        if (currentTipProvider != tipProvider) {
            // Зупиняємо попередню корутину, якщо вона існує
            if (hideCoroutine != null) {
                StopCoroutine(hideCoroutine);
            }

            // Викликаємо HideInfo для очищення попереднього тексту
            HideInfo(currentTipProvider);
        }

        // Зберігаємо поточного постачальника та відображаємо текст
        currentTipProvider = tipProvider;
        tipTextField.text = tipProvider.GetInfo();

        // Обчислюємо тривалість показу та запускаємо корутину для автоматичного приховування
        float displayDuration = CalculateDisplayDuration(tipProvider.GetInfo());
        hideCoroutine = StartCoroutine(HideInfoAfterDelay(displayDuration));
    }

    public void HideInfo(ITipProvider tipProvider) {
        if (tipProvider != currentTipProvider) return;

        // Очищуємо текст і скидаємо стан
        tipTextField.text = string.Empty;
        currentTipProvider = null;

        // Зупиняємо корутину, якщо вона існує
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
