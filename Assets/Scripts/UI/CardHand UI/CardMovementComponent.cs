using System.Collections;
using UnityEngine;

public class CardMovementComponent : MonoBehaviour {
    [Header("Movement Settings")]
    [SerializeField] private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 1f);

    [Header("Physics")]
    [SerializeField] private bool usePhysicsMovement = false;
    [SerializeField] private float springForce = 50f;
    [SerializeField] private float damping = 10f;

    private const float defaultMoveDuration = 0.5f;
    // State
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private Vector3 targetScale;
    private bool isMoving = false;
    private Coroutine currentMovement;

    // Events
    public System.Action OnMovementStarted;
    public System.Action OnMovementCompleted;

    // Components
    private CardTiltController tiltController;
    private CardPresenter cardPresenter;

    private void Awake() {
        targetPosition = transform.position;
        targetRotation = transform.rotation;
        targetScale = transform.localScale;

        tiltController = GetComponent<CardTiltController>();
        cardPresenter = GetComponent<CardPresenter>();
    }

    #region Public API - інші модулі використовують тільки ці методи

    /// <summary>
    /// Плавний рух до позиції з додатковими параметрами
    /// </summary>
    public void MoveTo(Vector3 position, Quaternion rotation, Vector3 scale, float duration = defaultMoveDuration, System.Action onComplete = null) {
        targetPosition = position;
        targetRotation = rotation;
        targetScale = scale;

        if (currentMovement != null) {
            StopCoroutine(currentMovement);
        }

        currentMovement = StartCoroutine(SmoothMovement(duration, onComplete));
    }


    /// <summary>
    /// Миттєве переміщення (без анімації)
    /// </summary>
    public void SetPosition(Vector3 position, Quaternion rotation, Vector3 scale) {
        if (currentMovement != null) {
            StopCoroutine(currentMovement);
            currentMovement = null;
        }

        targetPosition = position;
        targetRotation = rotation;
        targetScale = scale;

        transform.position = position;
        transform.rotation = rotation;
        transform.localScale = scale;
    }

    /// <summary>
    /// Фізичний рух (для драга)
    /// </summary>
    public void StartPhysicsMovement(Vector3 startPosition) {
        usePhysicsMovement = true;
        targetPosition = startPosition;
    }

    public void StopPhysicsMovement() {
        usePhysicsMovement = false;
    }

    /// <summary>
    /// Оновлення цільової позиції (для real-time руху)
    /// </summary>
    public void UpdateTargetPosition(Vector3 position) {
        targetPosition = position;
    }

    /// <summary>
    /// Зупинка всього руху
    /// </summary>
    public void StopMovement() {
        if (currentMovement != null) {
            StopCoroutine(currentMovement);
            currentMovement = null;
        }
        isMoving = false;
        usePhysicsMovement = false;
    }

    #endregion

    private void Update() {
        if (usePhysicsMovement) {
            UpdatePhysicsMovement();
        }
    }

    private void UpdatePhysicsMovement() {
        // Spring-based movement для smooth drag
        Vector3 force = (targetPosition - transform.position) * springForce;
        Vector3 velocity = force * Time.deltaTime;
        velocity *= (1f - damping * Time.deltaTime);

        transform.position += velocity * Time.deltaTime;

        // Оновлення tilt controller якщо є
        tiltController?.UpdateTilt(velocity);
    }

    private IEnumerator SmoothMovement(float duration, System.Action onComplete) {
        isMoving = true;
        OnMovementStarted?.Invoke();

        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;
        Vector3 startScale = transform.localScale;

        float elapsed = 0f;

        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Застосування кривих для більш природного руху
            float positionT = moveCurve.Evaluate(t);
            float scaleT = scaleCurve.Evaluate(t);

            transform.position = Vector3.Lerp(startPosition, targetPosition, positionT);
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
            transform.localScale = Vector3.Lerp(startScale, targetScale, scaleT);

            yield return null;
        }

        // Забезпечуємо точне досягнення цільових значень
        transform.position = targetPosition;
        transform.rotation = targetRotation;
        transform.localScale = targetScale;

        isMoving = false;
        currentMovement = null;

        OnMovementCompleted?.Invoke();
        onComplete?.Invoke();
    }

    #region Public Properties

    public bool IsMoving => isMoving || usePhysicsMovement;
    public Vector3 TargetPosition => targetPosition;
    public Vector3 CurrentVelocity { get; private set; }

    #endregion
}

