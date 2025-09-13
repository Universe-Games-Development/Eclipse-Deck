using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Threading;
using TMPro;
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

//public class PhysicsBasedStrategy : BaseMovementStrategy, IContinuousMovementStrategy {
//    private struct PhysicsState {
//        public Vector3 velocity;
//        public float adaptiveSpringForce;
//        public float dampingMultiplier;
//        public bool isSettling;

//        public PhysicsState(float springForce) {
//            velocity = Vector3.zero;
//            adaptiveSpringForce = springForce;
//            dampingMultiplier = 1f;
//            isSettling = false;
//        }
//    }

//    private PhysicsState physicsState;
//    private MovementTarget target;
//    private bool isActive;
//    private float settlementStartTime;

//    // Константи для налаштування поведінки
//    private const float PROXIMITY_THRESHOLD_MULTIPLIER = 2f;
//    private const float MIN_DAMPING_FACTOR = 0.3f;
//    private const float VELOCITY_DIRECTION_THRESHOLD = -0.1f;
//    private const float SETTLEMENT_DURATION = 0.3f;
//    private const float SETTLEMENT_LERP_SPEED = 8f;

//    public PhysicsBasedStrategy(MovementComponent movementComponent) : base(movementComponent) { }

//    public override async UniTask MoveToTargetAsync(Vector3 position, Quaternion rotation, Vector3 scale,
//        float duration, CancellationToken cancellationToken) {
//        InitializeMovement(new MovementTarget(position, rotation, scale));

//        try {
//            await UniTask.WaitUntil(IsMovementComplete, cancellationToken: cancellationToken);
//        } catch (OperationCanceledException) {
//            ResetSettlement();
//            throw;
//        }
//    }

//    public void UpdateTarget(MovementTarget newTarget) {
//        target = newTarget;

//        if (IsAtTarget()) {
//            StopMovement();
//            return;
//        }

//        if (!isActive) {
//            InitializeMovement(newTarget);
//        } else {
//            // Перезапускаємо рух якщо ціль змінилась значно
//            ResetSettlement();
//            Context.SetState(MovementState.Moving);
//        }
//    }

//    public override void Update() {
//        if (!isActive || Context.IsPaused) return;

//        if (physicsState.isSettling) {
//            UpdateSettlement();
//        } else {
//            UpdatePhysicsMovement();
//            CheckForSettlement();
//        }
//    }

//    private void InitializeMovement(MovementTarget newTarget) {
//        target = newTarget;
//        physicsState = new PhysicsState(Context.SpringForce);
//        isActive = true;
//        Context.SetState(MovementState.Moving);
//    }

//    private void UpdatePhysicsMovement() {
//        var (acceleration, updatedState) = CalculatePhysicsForces(physicsState);
//        physicsState = updatedState;

//        // Інтегруємо швидкість та позицію
//        physicsState.velocity += acceleration * Time.deltaTime;
//        ClampVelocity();

//        Context.transform.position += physicsState.velocity * Time.deltaTime;
//        Context.SetVelocity(physicsState.velocity);

//        //UpdateRotationAndScale();
//    }

//    private (Vector3 acceleration, PhysicsState updatedState) CalculatePhysicsForces(PhysicsState state) {
//        Vector3 displacement = target.position - Context.transform.position;
//        float distance = displacement.magnitude;

//        // Адаптивна пружинна сила з урахуванням відстані
//        float adaptiveForce = CalculateAdaptiveSpringForce(distance);
//        float dampingMultiplier = CalculateDampingMultiplier(distance);

//        Vector3 springForce = displacement.normalized * adaptiveForce;
//        Vector3 dampingForce = -state.velocity * (Context.Damping * dampingMultiplier);

//        var updatedState = state;

//        return (springForce + dampingForce, updatedState);
//    }

//    private float CalculateAdaptiveSpringForce(float distance) {
//        float proximityThreshold = Context.StoppingDistance * PROXIMITY_THRESHOLD_MULTIPLIER;

//        if (distance < proximityThreshold) {
//            float proximityFactor = Mathf.Clamp01(distance / proximityThreshold);
//            return Context.SpringForce * Mathf.SmoothStep(0.1f, 1f, proximityFactor);
//        }

//        return Context.SpringForce;
//    }

//    private float CalculateDampingMultiplier(float distance) {
//        float proximityThreshold = Context.StoppingDistance * PROXIMITY_THRESHOLD_MULTIPLIER;

//        if (distance < proximityThreshold) {
//            float proximityFactor = Mathf.Clamp01(distance / proximityThreshold);
//            return Mathf.Lerp(MIN_DAMPING_FACTOR, 1f, proximityFactor);
//        }

//        return 1f;
//    }

//    private void ClampVelocity() {
//        if (physicsState.velocity.magnitude > Context.MaxPhysicsSpeed) {
//            physicsState.velocity = physicsState.velocity.normalized * Context.MaxPhysicsSpeed;
//        }
//    }

//    private void UpdateRotationAndScale() {
//        UpdateRotation();
//        UpdateScale();
//    }

//    private void UpdateRotation() {
//        Quaternion targetRotation = Context.LookAtTarget && ShouldLookAtMovementDirection()
//            ? Quaternion.LookRotation(physicsState.velocity.normalized)
//            : target.rotation;

//        Context.transform.rotation = Quaternion.Slerp(
//            Context.transform.rotation,
//            targetRotation,
//            Context.RotationSpeed * Time.deltaTime
//        );
//    }

//    private bool ShouldLookAtMovementDirection() {
//        return physicsState.velocity.magnitude > Context.MinRotationVelocity;
//    }

//    private void UpdateScale() {
//        Context.transform.localScale = Vector3.Lerp(
//            Context.transform.localScale,
//            target.scale,
//            Context.MoveSpeed * Time.deltaTime
//        );
//    }

//    private void CheckForSettlement() {
//        if (ShouldStartSettlement()) {
//            StartSettlement();
//        }
//    }

//    private bool ShouldStartSettlement() {
//        return IsNearTarget() &&
//               IsVelocityLow() &&
//               !IsMovingAwayFromTarget();
//    }

//    private bool IsNearTarget() {
//        return Vector3.Distance(Context.transform.position, target.position) <= Context.StoppingDistance;
//    }

//    private bool IsVelocityLow() {
//        return physicsState.velocity.magnitude < Context.PhysicsStoppingVelocity * 0.5f;
//    }

//    private bool IsMovingAwayFromTarget() {
//        if (physicsState.velocity.magnitude < 0.01f) return false;

//        Vector3 directionToTarget = (target.position - Context.transform.position).normalized;
//        float dotProduct = Vector3.Dot(physicsState.velocity.normalized, directionToTarget);

//        return dotProduct < VELOCITY_DIRECTION_THRESHOLD;
//    }

//    private void StartSettlement() {
//        physicsState.isSettling = true;
//        settlementStartTime = Time.time;
//    }

//    private void UpdateSettlement() {
//        float settlementProgress = Mathf.Clamp01((Time.time - settlementStartTime) / SETTLEMENT_DURATION);
//        float lerpSpeed = SETTLEMENT_LERP_SPEED * Time.deltaTime;

//        // Плавно зменшуємо швидкість
//        physicsState.velocity = Vector3.Lerp(physicsState.velocity, Vector3.zero, lerpSpeed);

//        // Плавно наближаємося до цільової позиції
//        Context.transform.position = Vector3.Lerp(Context.transform.position, target.position, lerpSpeed);
//        //Context.transform.rotation = Quaternion.Slerp(Context.transform.rotation, target.rotation, lerpSpeed);
//        //Context.transform.localScale = Vector3.Lerp(Context.transform.localScale, target.scale, lerpSpeed);

//        Context.SetVelocity(physicsState.velocity);

//        // Завершуємо рух коли досягли достатньої точності
//        if (settlementProgress >= 1f || IsSettlementComplete()) {
//            CompleteMovement();
//        }
//    }

//    private bool IsSettlementComplete() {
//        return Vector3.Distance(Context.transform.position, target.position) < 0.001f &&
//               physicsState.velocity.magnitude < 0.001f;
//    }

//    private void CompleteMovement() {
//        // Встановлюємо точні фінальні значення
//        Context.transform.position = target.position;
//        //Context.transform.rotation = target.rotation;
//        //Context.transform.localScale = target.scale;

//        StopMovement();
//    }

//    private bool IsAtTarget() {
//        return Vector3.Distance(Context.transform.position, target.position) <= Context.StoppingDistance;
//    }

//    private void ResetSettlement() {
//        physicsState.isSettling = false;
//    }

//    private bool IsMovementComplete() {
//        return !isActive;
//    }

//    private void StopMovement() {
//        isActive = false;
//        physicsState.velocity = Vector3.zero;
//        physicsState.isSettling = false;
//        Context.SetVelocity(Vector3.zero);
//        Context.SetState(MovementState.Idle);
//    }
//}