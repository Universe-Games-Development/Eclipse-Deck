using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Threading;
using UnityEngine;

public enum MovementState {
    Idle,
    Moving,
    Paused
}

public class MovementComponent : MonoBehaviour {
    [Header("Movement Settings")]
    [SerializeField] private float _continuousMoveSpeed = 10f;

    // События для уведомления о состоянии движения
    public event System.Action OnMovementStarted;
    public event System.Action OnMovementCompleted;
    public event System.Action OnMovementCancelled;

    private CancellationTokenSource _moveCts;
    private bool _isContinuousMoveActive;
    private Vector3 _continuousMoveTarget;
    private Tweener _currentTween;

    public Vector3 CurrentVelocity { get; private set; }
    public MovementState State { get; private set; } = MovementState.Idle;
    public bool IsMoving => State == MovementState.Moving;

    [Header("Default Tween Settings")]
    [SerializeField] private float _defaultDuration = 1f;
    [SerializeField] private Ease _defaultEase = Ease.OutQuad;
    [SerializeField] private bool _useRelativeRotation = false;

    private void Update() {
        if (_isContinuousMoveActive) {
            PerformContinuousMove();

        }

        UpdateVelocity();
    }

    /// <summary>
    /// Выполняет переданный твин и управляет состоянием движения
    /// </summary>
    public async UniTask ExecuteTween(Tweener tween, CancellationToken externalToken = default) {
        // Останавливаем текущее движение
        StopMovement();

        _moveCts = CancellationTokenSource.CreateLinkedTokenSource(externalToken);
        _currentTween = tween;

        SetMovementState(MovementState.Moving);
        OnMovementStarted?.Invoke();

        try {
            // Ждем завершения твина
            await tween
                .SetLink(gameObject)
                .Play()
                .ToUniTask(TweenCancelBehaviour.Kill, _moveCts.Token);

            SetMovementState(MovementState.Idle);
            OnMovementCompleted?.Invoke();

        } catch (OperationCanceledException) when (_moveCts.Token.IsCancellationRequested) {
            SetMovementState(MovementState.Idle);
            OnMovementCancelled?.Invoke();
            DOTween.Kill(transform);
        }
    }

    /// <summary>
    /// Выполняет последовательность твинов
    /// </summary>
    public async UniTask ExecuteTweenSequence(Sequence sequence, CancellationToken externalToken = default) {
        StopMovement();
        
        _moveCts = CancellationTokenSource.CreateLinkedTokenSource(externalToken);

        SetMovementState(MovementState.Moving);
        OnMovementStarted?.Invoke();

        try {
            await sequence
                .SetLink(gameObject)
                .Play()
                .ToUniTask(TweenCancelBehaviour.Kill, _moveCts.Token);

            SetMovementState(MovementState.Idle);
            OnMovementCompleted?.Invoke();

        } catch (OperationCanceledException) when (_moveCts.Token.IsCancellationRequested) {
            SetMovementState(MovementState.Idle);
            OnMovementCancelled?.Invoke();
            DOTween.Kill(transform);
        }
    }

    public async UniTask MoveToPosition(Vector3 targetPosition, float? duration = null,
    Ease? ease = null, CancellationToken externalToken = default) {

        var tweener = transform.DOMove(targetPosition, duration ?? _defaultDuration)
            .SetEase(ease ?? _defaultEase);

        await ExecuteTween(tweener, externalToken);
    }

    /// <summary>
    /// Комплексне переміщення з усіма параметрами
    /// </summary>
    public async UniTask MoveTo(Vector3? position = null, Vector3? rotation = null,
        Vector3? scale = null, float? duration = null, Ease? ease = null,
        CancellationToken externalToken = default) {

        var sequence = DOTween.Sequence();

        if (position.HasValue) {
            await sequence.Join(transform.DOMove(position.Value, duration ?? _defaultDuration));
        }

        if (rotation.HasValue) {
            Tweener rotTween = _useRelativeRotation
                ? transform.DORotate(rotation.Value, duration ?? _defaultDuration)
                : transform.DORotateQuaternion(Quaternion.Euler(rotation.Value), duration ?? _defaultDuration);
            await sequence.Join(rotTween);
        }

        if (scale.HasValue) {
            await sequence.Join(transform.DOScale(scale.Value, duration ?? _defaultDuration));
        }

        await sequence.SetEase(ease ?? _defaultEase);

        await ExecuteTweenSequence(sequence, externalToken);
    }

    /// <summary>
    /// Запускает непрерывное движение к цели (для аналоговых контроллеров, AI и т.д.)
    /// </summary>
    public void StartContinuousMovement(Vector3 targetPosition) {
        StopTweenMovement();

        _continuousMoveTarget = targetPosition;
        _isContinuousMoveActive = true;

        SetMovementState(MovementState.Moving);
        OnMovementStarted?.Invoke();
    }

    /// <summary>
    /// Обновляет цель непрерывного движения
    /// </summary>
    public void UpdateContinuousTarget(Vector3 newTarget) {
        if (_isContinuousMoveActive) {
            _continuousMoveTarget = newTarget;
        } else {
            StartContinuousMovement(newTarget);
        }
    }

    /// <summary>
    /// Останавливает любое движение
    /// </summary>
    public void StopMovement() {
        StopContinuousMovement();
        StopTweenMovement();

        SetMovementState(MovementState.Idle);
        OnMovementCompleted?.Invoke();
    }

    // Останавливаем непрерывное движение
    public void StopContinuousMovement() {
        if (_isContinuousMoveActive) {
            _isContinuousMoveActive = false;
        }
    }

    // Отменяем твины
    public void StopTweenMovement() {
        _moveCts?.Cancel();
    }

    /// <summary>
    /// Приостанавливает движение (только для твинов)
    /// </summary>
    public void PauseMovement() {
        if (State == MovementState.Moving && _currentTween != null && _currentTween.IsActive()) {
            _currentTween.Pause();
            SetMovementState(MovementState.Paused);
        }
    }

    /// <summary>
    /// Возобновляет приостановленное движение
    /// </summary>
    public void ResumeMovement() {
        if (State == MovementState.Paused && _currentTween != null && _currentTween.IsActive()) {
            _currentTween.Play();
            SetMovementState(MovementState.Moving);
        }
    }

    private void PerformContinuousMove() {
        Vector3 oldPosition = transform.position;

        Vector3 newPosition = Vector3.Lerp(
            transform.position,
            _continuousMoveTarget,
            _continuousMoveSpeed * Time.deltaTime
        );

        transform.position = newPosition;

        // Проверяем, достигли ли цели
        float distanceToTarget = Vector3.Distance(transform.position, _continuousMoveTarget);
        if (distanceToTarget < 0.01f) {
            _isContinuousMoveActive = false;
            SetMovementState(MovementState.Idle);
            OnMovementCompleted?.Invoke();
        }
    }

    private void UpdateVelocity() {
        // Обновляем скорость на основе изменения позиции
        Vector3 currentPosition = transform.position;
        CurrentVelocity = (currentPosition - _lastPosition) / Time.deltaTime;
        _lastPosition = currentPosition;
    }

    private Vector3 _lastPosition;

    private void SetMovementState(MovementState newState) {
        if (State != newState) {
            State = newState;
        }
    }

    private void Start() {
        _lastPosition = transform.position;
    }

    private void OnDestroy() {
        _moveCts?.Cancel();
        _moveCts?.Dispose();
        DOTween.Kill(transform);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected() {
        if (!Application.isPlaying) return;

        // Показываем цель непрерывного движения
        if (_isContinuousMoveActive) {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_continuousMoveTarget, 0.2f);

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, _continuousMoveTarget);
        }

        // Показываем текущую скорость
        if (CurrentVelocity.magnitude > 0.1f) {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, CurrentVelocity);
        }

        // Показываем состояние
        Gizmos.color = State switch {
            MovementState.Moving => Color.green,
            MovementState.Paused => Color.yellow,
            _ => Color.gray
        };
        Gizmos.DrawWireCube(transform.position + Vector3.up * 2, Vector3.one * 0.3f);
    }
#endif
}
