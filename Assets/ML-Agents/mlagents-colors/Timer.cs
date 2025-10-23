using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class Timer : MonoBehaviour {
    [SerializeField] private float duration;
    [SerializeField] private bool autoStart = false;

    public event Action<string> OnTimeUpdated;
    public event Action OnTimerCompleted;
    public event Action OnTimerCancelled;

    private CancellationTokenSource _cancellationTokenSource;
    private bool _isRunning;

    public float CurrentTime { get; private set; }
    public bool IsRunning => _isRunning;

    private void Start() {
        if (autoStart) {
            StartTimer();
        }
    }

    private void OnDestroy() {
        CancelTimer();
    }

    public void StartTimer(float? customDuration = null) {
        if (_isRunning) return;

        duration = customDuration ?? duration;
        CurrentTime = duration;
        _cancellationTokenSource = new CancellationTokenSource();

        _isRunning = true;
        RunTimerAsync(_cancellationTokenSource.Token).Forget();
    }

    public void CancelTimer() {
        if (!_isRunning) return;

        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
        _isRunning = false;

        OnTimerCancelled?.Invoke();
    }

    public void PauseTimer() {
        if (!_isRunning) return;

        _cancellationTokenSource?.Cancel();
        _isRunning = false;
    }

    public void ResumeTimer() {
        if (_isRunning) return;

        StartTimer(CurrentTime);
    }

    public void ResetTimer() {
        CancelTimer();
        CurrentTime = duration;
        UpdateTimeDisplay();
    }

    private async UniTaskVoid RunTimerAsync(CancellationToken cancellationToken) {
        try {
            while (CurrentTime > 0f) {
                await UniTask.NextFrame(cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                    return;

                CurrentTime -= Time.deltaTime;
                UpdateTimeDisplay();

                if (CurrentTime <= 0f) {
                    CurrentTime = 0f;
                    break;
                }
            }

            if (!cancellationToken.IsCancellationRequested) {
                _isRunning = false;
                OnTimerCompleted?.Invoke();
                UpdateTimeDisplay();
            }
        } catch (OperationCanceledException) {
            // Timer was cancelled, this is expected
        }
    }

    private void UpdateTimeDisplay() {
        string formattedTime = FormatTime(CurrentTime);
        OnTimeUpdated?.Invoke(formattedTime);
    }

    public static string FormatTime(float timeInSeconds) {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        return $"{minutes:00}:{seconds:00}";
    }
}