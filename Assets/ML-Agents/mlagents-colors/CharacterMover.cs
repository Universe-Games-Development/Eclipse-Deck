using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CharacterMover : MonoBehaviour {
    [Header("Налаштування руху")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float turnSpeed = 180f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 8f;

    [Header("Згладжування повороту")]
    [Tooltip("Швидкість згладжування повороту (більше = плавніше)")]
    [SerializeField] private float turnSmoothness = 5f;

    [Tooltip("Мінімальний вхід для повороту (уникнення тремтіння)")]
    [SerializeField] private float turnDeadzone = 0.05f;

    [Header("Згладжування руху")]
    [Tooltip("Швидкість згладжування зміни напрямку")]
    [SerializeField] private float directionSmoothness = 8f;

    [Tooltip("Мінімальний вхід для руху")]
    [SerializeField] private float moveDeadzone = 0.05f;

    [Header("Фізика")]
    [SerializeField] private float maxSlopeAngle = 45f;
    [SerializeField] private LayerMask groundLayerMask = 1;
    [SerializeField] private float groundRayLength = 0.2f;
    [SerializeField] private Transform legs;

    private Rigidbody _rb;
    private Vector3 _moveInput;
    private Vector3 _smoothedMoveInput;
    private float _turnInput;
    private float _smoothedTurnInput;
    private Vector3 _currentVelocity;
    private bool _isGrounded;
    private Vector3 _lastGroundNormal;

    // Додаткові змінні для плавного повороту
    private float _currentAngularVelocity;
    private float _targetRotationVelocity;

    // Публічні властивості
    public float CurrentSpeed => _currentVelocity.magnitude;
    public Vector3 CurrentVelocity => _currentVelocity;
    public bool IsMoving => _smoothedMoveInput.magnitude > moveDeadzone;
    public bool IsGrounded => _isGrounded;

    private void Awake() {
        _rb = GetComponent<Rigidbody>();
    }

    private void Start() {
        if (_rb != null) {
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
            _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            // Заморожуємо обертання по X та Z для стабільності
            _rb.freezeRotation = true;
        }
    }

    private void FixedUpdate() {
        Debug.DrawRay(transform.position, CurrentVelocity.normalized * 1.5f, Color.red);
        CheckGrounded();
        SmoothInputs();
        ProcessMovement();
        ProcessTurning();
    }

    /// <summary>
    /// Згладжування вхідних даних для плавнішого руху
    /// </summary>
    private void SmoothInputs() {
        // Згладжування вхідних даних для руху
        _smoothedMoveInput = Vector3.Lerp(
            _smoothedMoveInput,
            _moveInput,
            directionSmoothness * Time.fixedDeltaTime
        );

        // Застосування deadzone для руху
        if (_smoothedMoveInput.magnitude < moveDeadzone) {
            _smoothedMoveInput = Vector3.zero;
        }

        // Згладжування вхідних даних для повороту
        _smoothedTurnInput = Mathf.Lerp(
            _smoothedTurnInput,
            _turnInput,
            turnSmoothness * Time.fixedDeltaTime
        );

        // Застосування deadzone для повороту
        if (Mathf.Abs(_smoothedTurnInput) < turnDeadzone) {
            _smoothedTurnInput = 0f;
        }
    }

    /// <summary>
    /// Встановити вхідні дані для руху (локальні координати)
    /// </summary>
    public void SetMovementInput(float forwardInput, float rightInput) {
        _moveInput = new Vector3(rightInput, 0f, forwardInput);

        // Обмежуємо величину вектора
        if (_moveInput.magnitude > 1f) {
            _moveInput.Normalize();
        }
    }

    /// <summary>
    /// Встановити вхідні дані для повороту
    /// </summary>
    public void SetTurnInput(float turnInput) {
        _turnInput = Mathf.Clamp(turnInput, -1f, 1f);
    }

    /// <summary>
    /// Отримати поточний напрямок руху
    /// </summary>
    public Vector3 GetMoveDirection() {
        return _currentVelocity.magnitude > 0.01f ? _currentVelocity.normalized : transform.forward;
    }

    /// <summary>
    /// Зупинити рух
    /// </summary>
    public void StopMovement() {
        _moveInput = Vector3.zero;
        _smoothedMoveInput = Vector3.zero;
        _turnInput = 0f;
        _smoothedTurnInput = 0f;
        _currentVelocity = Vector3.zero;
        _currentAngularVelocity = 0f;
        _targetRotationVelocity = 0f;

        if (_rb != null) {
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }
    }

    private void ProcessMovement() {
        if (!_isGrounded) return;

        // Конвертуємо локальний вхід у світові координати (використовуємо згладжений вхід)
        Vector3 worldMoveDirection = transform.TransformDirection(_smoothedMoveInput);

        // Проектуємо на поверхню
        if (_isGrounded) {
            worldMoveDirection = Vector3.ProjectOnPlane(worldMoveDirection, _lastGroundNormal).normalized;
        }

        // Цільова швидкість
        Vector3 targetVelocity = worldMoveDirection * (_smoothedMoveInput.magnitude * moveSpeed);

        // Вибираємо прискорення або гальмування
        float currentAcceleration = _smoothedMoveInput.magnitude > 0.1f ? acceleration : deceleration;

        // Плавна інтерполяція до цільової швидкості
        _currentVelocity = Vector3.Lerp(
            _currentVelocity,
            targetVelocity,
            currentAcceleration * Time.fixedDeltaTime
        );

        // Застосовуємо швидкість
        if (_rb != null) {
            Vector3 newVelocity = new Vector3(_currentVelocity.x, _rb.linearVelocity.y, _currentVelocity.z);
            _rb.linearVelocity = newVelocity;
        }
    }

    private void ProcessTurning() {
        // Використовуємо згладжений вхід
        if (Mathf.Abs(_smoothedTurnInput) < 0.001f) {
            // Якщо немає вхідних даних, плавно зупиняємо обертання
            _targetRotationVelocity = 0f;
        } else {
            // Обчислюємо цільову швидкість обертання
            _targetRotationVelocity = _smoothedTurnInput * turnSpeed;
        }

        // Плавно інтерполюємо поточну кутову швидкість до цільової
        _currentAngularVelocity = Mathf.Lerp(
            _currentAngularVelocity,
            _targetRotationVelocity,
            turnSmoothness * Time.fixedDeltaTime
        );

        // Застосовуємо обертання
        if (Mathf.Abs(_currentAngularVelocity) > 0.01f && _rb != null) {
            float turnAngle = _currentAngularVelocity * Time.fixedDeltaTime;
            Quaternion turnRotation = Quaternion.Euler(0f, turnAngle, 0f);
            _rb.MoveRotation(_rb.rotation * turnRotation);
        }
    }

    private void CheckGrounded() {
        Vector3 rayStart = legs != null ? legs.position : transform.position;

        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, groundRayLength, groundLayerMask)) {
            float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);

            if (slopeAngle <= maxSlopeAngle) {
                _isGrounded = true;
                _lastGroundNormal = hit.normal;
                return;
            }
        }

        _isGrounded = false;
        _lastGroundNormal = Vector3.up;
    }

    // Візуалізація для відладки
    private void OnDrawGizmosSelected() {
        if (!Application.isPlaying) return;

        // Напрямок руху (зелений)
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, GetMoveDirection() * 2f);

        // Згладжений вхід (синій)
        Gizmos.color = Color.blue;
        Vector3 worldSmoothInput = transform.TransformDirection(_smoothedMoveInput);
        Gizmos.DrawRay(transform.position, worldSmoothInput * 1.5f);

        // Необроблений вхід (жовтий, тонкий)
        Gizmos.color = Color.yellow;
        Vector3 worldRawInput = transform.TransformDirection(_moveInput);
        Gizmos.DrawRay(transform.position, worldRawInput * 1f);

        // Перевірка землі
        Vector3 rayStart = legs != null ? legs.transform.position : transform.position;
        Gizmos.DrawRay(rayStart, rayStart + Vector3.down * groundRayLength);
    }
}