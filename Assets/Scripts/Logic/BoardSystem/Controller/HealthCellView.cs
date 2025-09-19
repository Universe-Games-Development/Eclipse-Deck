using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class HealthCellView : MonoBehaviour {
    [SerializeField] private MeshRenderer liquidRenderer;
    [SerializeField] private AnimationCurve animationCurve = AnimationCurve.Linear(0, 0, 1, 1);
    [SerializeField] private float maxLevel = 1.5f;
    [SerializeField] private float minLevel = -1.5f;
    [SerializeField] private float duration = 2f;

    private Health health;
    private MaterialPropertyBlock propertyBlock;
    private CancellationTokenSource animationCTS;
    private const string LevelProperty = "_Level";

    public void Initialize() {
        if (liquidRenderer == null)
            Debug.LogError("Liquid Renderer is not assigned!");
        InitializePropertyBlock();
    }

    private void InitializePropertyBlock() {
        propertyBlock = new MaterialPropertyBlock();
        liquidRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetFloat(LevelProperty, 0);
        liquidRenderer.SetPropertyBlock(propertyBlock);
    }

    public void AssignOwner(IHealthable healthable) {
        if (healthable?.Health == null) {
            Debug.LogError("Invalid opponent or health component!");
            return;
        }

        UnsubscribeFromPreviousHealth();
        health = healthable.Health;

        SubscribeToHealthEvents();
        SmoothUpdateLiquidLevel(health.Current, health.Current).Forget(); // Встановлюємо початкове значення
    }

    private void SubscribeToHealthEvents() {
        health.OnTotalValueChanged += UpdateHealthBar;
    }

    private void UpdateHealthBar(object sender, AttributeTotalChangedEvent eventData) {
        SmoothUpdateLiquidLevel(eventData.NewValue, eventData.OldValue).Forget();
    }

    private void UnsubscribeFromPreviousHealth() {
        if (health != null) {
            health.OnTotalValueChanged -= UpdateHealthBar;
        }
    }

    private async UniTaskVoid SmoothUpdateLiquidLevel(int newValue, int previousValue) {
        CancelCurrentAnimation();

        // Створюємо НОВИЙ CTS для цієї конкретної анімації
        var localCTS = new CancellationTokenSource();
        animationCTS = localCTS; // Записуємо посилання в поле класу
        var token = localCTS.Token;

        try {
            liquidRenderer.GetPropertyBlock(propertyBlock);
            float startLevel = propertyBlock.GetFloat(LevelProperty);
            float targetLevel = CalculateLiquidLevel(newValue);

            float elapsed = 0f;
            while (elapsed < duration) {
                if (token.IsCancellationRequested) break;

                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float curvedT = animationCurve.Evaluate(t);
                UpdateLiquidLevel(Mathf.Lerp(startLevel, targetLevel, curvedT));
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

            if (!token.IsCancellationRequested) {
                UpdateLiquidLevel(targetLevel);
            }
        } catch (OperationCanceledException) { } finally {
            // Видаляємо тільки якщо це ТЕКУЩА анімація
            if (animationCTS == localCTS) {
                localCTS.Dispose();
                animationCTS = null;
            }
        }
    }

    private void CancelCurrentAnimation() {
        animationCTS?.Cancel();
        animationCTS?.Dispose();
        animationCTS = null;
    }

    private void UpdateLiquidLevel(float level) {
        if (liquidRenderer == null) return;

        liquidRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetFloat(LevelProperty, level);
        liquidRenderer.SetPropertyBlock(propertyBlock);
    }

    // Оновлений метод для розрахунку рівня рідини з урахуванням конкретного значення здоров'я
    private float CalculateLiquidLevel(int currentHealth) {
        if (health == null || health.TotalValue <= 0) return minLevel;

        float normalizedLevel = (float)currentHealth / health.TotalValue;
        return Mathf.Lerp(minLevel, maxLevel, normalizedLevel);
    }

    // Оригінальний метод, використовує поточне значення здоров'я
    private float CalculateLiquidLevel() {
        if (health == null || health.TotalValue <= 0) return minLevel;

        float normalizedLevel = (float)health.Current / health.TotalValue;
        return Mathf.Lerp(minLevel, maxLevel, normalizedLevel);
    }

    private void OnDestroy() {
        CancelCurrentAnimation();
        UnsubscribeFromPreviousHealth();
    }

    public void ClearOwner() {
        UpdateLiquidLevel(0);
        health = null;
    }
}