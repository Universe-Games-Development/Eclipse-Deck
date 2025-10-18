using System;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ColorGame : MonoBehaviour {
    [SerializeField] float decisionTime = 10f;
    [SerializeField] ColorObjectsManager colorObjectsManager;
    [SerializeField] bool doRandomColors;
    [SerializeField] TextMeshPro timerText; // Референс до UI тексту для таймера

    [SerializeField] TextMeshPro targetColorName;
    
    ColorInfo targetInfo;
    [SerializeField] private Timer timer;
    private CancellationTokenSource gameCancellationTokenSource;
    [SerializeField] Transform spawnPoint;
    [SerializeField] bool updateObjects = false;

    private void Awake() {
        timer = GetComponent<Timer>();
        if (timer == null)
            timer = gameObject.AddComponent<Timer>();

        // Підписка на події таймера
        timer.OnTimeUpdated += UpdateTimerText;
        timer.OnTimerCompleted += OnTimerExpired;
    }

    private void OnDestroy() {
        if (timer != null) {
            timer.OnTimeUpdated -= UpdateTimerText;
            timer.OnTimerCompleted -= OnTimerExpired;
        }
        gameCancellationTokenSource?.Cancel();
        gameCancellationTokenSource?.Dispose();
    }

    public Action OnGameTimeExpired;
    private void OnTimerExpired() {
        OnGameTimeExpired?.Invoke();
    }

    public void StartGame() {
        // Створюємо новий CancellationToken для цієї гри
        gameCancellationTokenSource = new CancellationTokenSource();

        if (updateObjects || colorObjectsManager.GetObjectCount() == 0) {
            colorObjectsManager.Initialize(6);
        }
        
        if (doRandomColors)
            colorObjectsManager.RandomizeColors();

        
        SetNewColor();

        // Запускаємо таймер
        timer.StartTimer(decisionTime);
    }

    public bool TryChooseColor(Color color) {
        if (targetInfo != null) {
            bool isCorrect = CompareColors(targetInfo.color, color);
            if (isCorrect) {
                timer.CancelTimer(); // Зупиняємо таймер при правильній відповіді
            }
            return isCorrect;
        }
        Debug.Log("target color is null");
        return false;
    }

    public void ResetGame() {
        targetInfo = null;
        timer?.CancelTimer();
        timer?.ResetTimer();
    }

    private void SetNewColor() {
        targetInfo = colorObjectsManager.CreateNewTargetColor();
        //Debug.Log($"New target color: {targetInfo.colorName}");
        if (targetColorName != null) {
            targetColorName.text = targetInfo.colorName;
            Color targetCOlor = targetInfo.color;
            targetCOlor.a = 1f;
            targetColorName.color = targetCOlor;
        }
    }

    public ColorInfo GetTargetColor() {
        return targetInfo;
    }

    private void UpdateTimerText(string timeString) {
        if (timerText != null)
            timerText.text = timeString;
    }

    private bool CompareColors(Color color1, Color color2, float tolerance = 0.1f) {
        return Mathf.Abs(color1.r - color2.r) < tolerance &&
               Mathf.Abs(color1.g - color2.g) < tolerance &&
               Mathf.Abs(color1.b - color2.b) < tolerance;
    }

    public List<ColorInfo> GetAllColors() {
        return colorObjectsManager.GetCurrentColors();
    }

    public ColorObject GetColorObject(ColorInfo colorInfo) {
        return colorObjectsManager.GetColorObjectByColor(colorInfo.color);
    }

    public ColorObject GetRightObject() {
        return colorObjectsManager.GetColorObjectByColor(targetInfo.color);
    }
}
