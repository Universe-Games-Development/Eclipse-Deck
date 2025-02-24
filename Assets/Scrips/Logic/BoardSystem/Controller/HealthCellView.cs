using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class HealthCellView : MonoBehaviour {
    [SerializeField] private MeshRenderer liquidRenderer;
    [SerializeField] private AnimationCurve animationCurve = AnimationCurve.Linear(0, 0, 1, 1);
    [SerializeField] private float maxLevel = 1.5f;
    [SerializeField] private float minLevel = -1.5f;

    private Health health;
    private MaterialPropertyBlock propertyBlock;
    private CancellationTokenSource animationCTS;
    private const string LevelProperty = "_Level";

    private void Awake() {
        ValidateComponents();
        InitializePropertyBlock();
    }

    private void ValidateComponents() {
        if (liquidRenderer == null)
            Debug.LogError("Liquid Renderer is not assigned!");
    }

    private void InitializePropertyBlock() {
        propertyBlock = new MaterialPropertyBlock();
        liquidRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetFloat(LevelProperty, 0);
        liquidRenderer.SetPropertyBlock(propertyBlock);
    }

    public void AssignOwner(Opponent opponent) {
        if (opponent?.Health == null) {
            Debug.LogError("Invalid opponent or health component!");
            return;
        }

        UnsubscribeFromPreviousHealth();
        health = opponent.Health;

        SubscribeToHealthEvents();
        UpdateVisualImmediately();
    }

    private void SubscribeToHealthEvents() {
        health.Stat.OnValueChanged += UpdateHealthBar;
        health.OnChangedMaxValue += HandleMaxHealthChanged;
    }

    private void UnsubscribeFromPreviousHealth() {
        if (health != null) {
            health.Stat.OnValueChanged -= UpdateHealthBar;
            health.OnChangedMaxValue -= HandleMaxHealthChanged;
        }
    }

    private void UpdateVisualImmediately() {
        CancelCurrentAnimation();
        UpdateLiquidLevel(CalculateLiquidLevel());
    }

    private void HandleMaxHealthChanged(int previousMax, int newMax) {
        if (newMax <= 0) return;
        SmoothUpdateLiquidLevel().Forget();
    }

    private void UpdateHealthBar(int previousValue, int newValue) {
        SmoothUpdateLiquidLevel().Forget();
    }

    private async UniTaskVoid SmoothUpdateLiquidLevel() {
        
        using var animationCTS = new CancellationTokenSource();

        try {
            CancelCurrentAnimation();
            

            float targetLevel = CalculateLiquidLevel();
            float startLevel = propertyBlock.GetFloat(LevelProperty);
            float duration = 0.5f;
            float elapsed = 0f;

            while (elapsed < duration && !animationCTS.Token.IsCancellationRequested) {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float curvedT = animationCurve.Evaluate(t);
                UpdateLiquidLevel(Mathf.Lerp(startLevel, targetLevel, curvedT));
                await UniTask.Yield(PlayerLoopTiming.Update, animationCTS.Token);
            }

            if (!animationCTS.Token.IsCancellationRequested) {
                UpdateLiquidLevel(targetLevel);
            }
        } catch (OperationCanceledException) {
            // Animation canceled it`s normal
        }
    }

    private float CalculateLiquidLevel() {
        if (health == null || health.Max <= 0) return minLevel; // Встановлюємо мінімальний рівень при нульовому здоров'ї

        float normalizedLevel = (float)health.Current / health.Max; // Значення від 0 до 1
        return Mathf.Lerp(minLevel, maxLevel, normalizedLevel); // Масштабуємо до [-1.5, 1.5]
    }


    private void UpdateLiquidLevel(float level) {
        if (liquidRenderer == null) return;

        liquidRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetFloat(LevelProperty, level);
        liquidRenderer.SetPropertyBlock(propertyBlock);
    }

    private void CancelCurrentAnimation() {
        animationCTS?.Cancel();
        animationCTS?.Dispose();
        animationCTS = null;
    }

    private void OnDestroy() {
        CancelCurrentAnimation();
        UnsubscribeFromPreviousHealth();
    }
}