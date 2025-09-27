using Cysharp.Threading.Tasks;
using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Splines;

public class CameraSplineMover : MonoBehaviour {
    public float duration = 5f;
    [SerializeField] private CinemachineSplineDolly dolly;
    [SerializeField] private float delayToMove;
    [SerializeField] private float endTime = 0f;
    [SerializeField] private AnimationCurve easingCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private bool useTimeScaleIndependent = true;
    [SerializeField] private float initialBlendTime = 0.5f;

    public Action OnMovementStart;
    public Action OnMovementComplete;

    private bool _isMoving;
    private Vector3 _initialPosition;
    private Quaternion _initialRotation;

    public async UniTask StartCameraMovementAsync(SplineContainer splineContainer) {
        if (dolly == null) {
            Debug.LogError("SplineComponent не встановлений!");
            return;
        }

        if (_isMoving) return;
        _isMoving = true;

        // Сохраняем начальное положение и поворот для плавного перехода
        _initialPosition = transform.position;
        _initialRotation = transform.rotation;

        dolly.Spline = splineContainer;
        OnMovementStart?.Invoke();


        // Затримка перед стартом руху
        if (delayToMove > 0f) {
            await UniTask.Delay(TimeSpan.FromSeconds(delayToMove), cancellationToken: this.GetCancellationTokenOnDestroy());
        }

        if (initialBlendTime > 0f) {
            await BlendToSplineAsync(initialBlendTime);
        }

        await MoveCameraAsync();

        if (endTime > 0f) {
            await UniTask.Delay(TimeSpan.FromSeconds(endTime), cancellationToken: this.GetCancellationTokenOnDestroy());
        }

        OnMovementComplete?.Invoke();
        _isMoving = false;
    }

    private async UniTask BlendToSplineAsync(float blendTime) {
        float elapsedTime = 0f;
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;

        // Установка начальной позиции камеры на сплайне
        dolly.CameraPosition = 0f;
        Vector3 targetPosition = dolly.transform.position;
        Quaternion targetRotation = dolly.transform.rotation;

        while (elapsedTime < blendTime) {
            float deltaTime = useTimeScaleIndependent ? Time.unscaledDeltaTime : Time.deltaTime;
            elapsedTime += deltaTime;
            float t = Mathf.Clamp01(elapsedTime / blendTime);
            float smoothT = easingCurve.Evaluate(t);

            transform.position = Vector3.Lerp(startPosition, targetPosition, smoothT);
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, smoothT);

            await UniTask.Yield(useTimeScaleIndependent ? PlayerLoopTiming.PostLateUpdate : PlayerLoopTiming.Update);
        }
    }

    private async UniTask MoveCameraAsync() {
        float elapsedTime = 0f;

        while (elapsedTime < duration) {
            float deltaTime = useTimeScaleIndependent ? Time.unscaledDeltaTime : Time.deltaTime;
            elapsedTime += deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsedTime / duration);

            // Применяем кривую сглаживания для плавности движения
            float smoothProgress = easingCurve.Evaluate(normalizedTime);
            dolly.CameraPosition = smoothProgress;

            await UniTask.Yield(useTimeScaleIndependent ? PlayerLoopTiming.PostLateUpdate : PlayerLoopTiming.Update);
        }

        // Забезпечити 100% прогрес в кінці
        dolly.CameraPosition = 1f;
    }

    public void StopMovement() {
        _isMoving = false;
    }
}