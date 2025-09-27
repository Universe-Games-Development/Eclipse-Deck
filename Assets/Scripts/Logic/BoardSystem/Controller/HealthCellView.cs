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

    private MaterialPropertyBlock propertyBlock;
    private CancellationTokenSource animationCTS;
    private const string LevelProperty = "_Level";

    private float _currentHealth;
    private float _maxHealth;

    private void Awake() {
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

    /// <summary>
    /// Оновлює стан хп ззовні (тепер головний спосіб).
    /// </summary>
    public void UpdateHealth(float health, float maxHealth) {
        if (maxHealth <= 0) {
            Debug.LogError("MaxHealth must be greater than zero");
            return;
        }

        float oldValue = _currentHealth;
        _currentHealth = health;
        _maxHealth = maxHealth;

        SmoothUpdateLiquidLevel(_currentHealth, oldValue).Forget();
    }

    private async UniTaskVoid SmoothUpdateLiquidLevel(float newValue, float previousValue) {
        CancelCurrentAnimation();

        var localCTS = new CancellationTokenSource();
        animationCTS = localCTS;
        var token = localCTS.Token;

        try {
            liquidRenderer.GetPropertyBlock(propertyBlock);
            float startLevel = propertyBlock.GetFloat(LevelProperty);
            float targetLevel = CalculateLiquidLevel(newValue, _maxHealth);

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
        } catch (OperationCanceledException) {
            // ігноруємо — це очікувано
        } finally {
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

    private float CalculateLiquidLevel(float currentHealth, float maxHealth) {
        if (maxHealth <= 0) return minLevel;
        float normalizedLevel = Mathf.Clamp01(currentHealth / maxHealth);
        return Mathf.Lerp(minLevel, maxLevel, normalizedLevel);
    }

    public void ClearOwner() {
        CancelCurrentAnimation();
        UpdateLiquidLevel(0);
        _currentHealth = 0;
        _maxHealth = 0;
    }

    private void OnDestroy() {
        CancelCurrentAnimation();
    }
}
