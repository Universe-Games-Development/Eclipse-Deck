using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CharacterMover : MonoBehaviour {
    [Header("������������ ����")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float turnSpeed = 180f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 8f;

    [Header("������������ ��������")]
    [Tooltip("�������� ������������ �������� (����� = �������)")]
    [SerializeField] private float turnSmoothness = 5f;

    [Tooltip("̳�������� ���� ��� �������� (��������� ��������)")]
    [SerializeField] private float turnDeadzone = 0.05f;

    [Header("������������ ����")]
    [Tooltip("�������� ������������ ���� ��������")]
    [SerializeField] private float directionSmoothness = 8f;

    [Tooltip("̳�������� ���� ��� ����")]
    [SerializeField] private float moveDeadzone = 0.05f;

    [Header("Գ����")]
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

    // �������� ���� ��� �������� ��������
    private float _currentAngularVelocity;
    private float _targetRotationVelocity;

    // ������ ����������
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
            // ���������� ��������� �� X �� Z ��� ����������
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
    /// ������������ ������� ����� ��� ��������� ����
    /// </summary>
    private void SmoothInputs() {
        // ������������ ������� ����� ��� ����
        _smoothedMoveInput = Vector3.Lerp(
            _smoothedMoveInput,
            _moveInput,
            directionSmoothness * Time.fixedDeltaTime
        );

        // ������������ deadzone ��� ����
        if (_smoothedMoveInput.magnitude < moveDeadzone) {
            _smoothedMoveInput = Vector3.zero;
        }

        // ������������ ������� ����� ��� ��������
        _smoothedTurnInput = Mathf.Lerp(
            _smoothedTurnInput,
            _turnInput,
            turnSmoothness * Time.fixedDeltaTime
        );

        // ������������ deadzone ��� ��������
        if (Mathf.Abs(_smoothedTurnInput) < turnDeadzone) {
            _smoothedTurnInput = 0f;
        }
    }

    /// <summary>
    /// ���������� ����� ��� ��� ���� (������� ����������)
    /// </summary>
    public void SetMovementInput(float forwardInput, float rightInput) {
        _moveInput = new Vector3(rightInput, 0f, forwardInput);

        // �������� �������� �������
        if (_moveInput.magnitude > 1f) {
            _moveInput.Normalize();
        }
    }

    /// <summary>
    /// ���������� ����� ��� ��� ��������
    /// </summary>
    public void SetTurnInput(float turnInput) {
        _turnInput = Mathf.Clamp(turnInput, -1f, 1f);
    }

    /// <summary>
    /// �������� �������� �������� ����
    /// </summary>
    public Vector3 GetMoveDirection() {
        return _currentVelocity.magnitude > 0.01f ? _currentVelocity.normalized : transform.forward;
    }

    /// <summary>
    /// �������� ���
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

        // ���������� ��������� ���� � ����� ���������� (������������� ���������� ����)
        Vector3 worldMoveDirection = transform.TransformDirection(_smoothedMoveInput);

        // ��������� �� ��������
        if (_isGrounded) {
            worldMoveDirection = Vector3.ProjectOnPlane(worldMoveDirection, _lastGroundNormal).normalized;
        }

        // ֳ����� ��������
        Vector3 targetVelocity = worldMoveDirection * (_smoothedMoveInput.magnitude * moveSpeed);

        // �������� ����������� ��� �����������
        float currentAcceleration = _smoothedMoveInput.magnitude > 0.1f ? acceleration : deceleration;

        // ������ ������������ �� ������� ��������
        _currentVelocity = Vector3.Lerp(
            _currentVelocity,
            targetVelocity,
            currentAcceleration * Time.fixedDeltaTime
        );

        // ����������� ��������
        if (_rb != null) {
            Vector3 newVelocity = new Vector3(_currentVelocity.x, _rb.linearVelocity.y, _currentVelocity.z);
            _rb.linearVelocity = newVelocity;
        }
    }

    private void ProcessTurning() {
        // ������������� ���������� ����
        if (Mathf.Abs(_smoothedTurnInput) < 0.001f) {
            // ���� ���� ������� �����, ������ ��������� ���������
            _targetRotationVelocity = 0f;
        } else {
            // ���������� ������� �������� ���������
            _targetRotationVelocity = _smoothedTurnInput * turnSpeed;
        }

        // ������ ������������ ������� ������ �������� �� �������
        _currentAngularVelocity = Mathf.Lerp(
            _currentAngularVelocity,
            _targetRotationVelocity,
            turnSmoothness * Time.fixedDeltaTime
        );

        // ����������� ���������
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

    // ³��������� ��� �������
    private void OnDrawGizmosSelected() {
        if (!Application.isPlaying) return;

        // �������� ���� (�������)
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, GetMoveDirection() * 2f);

        // ���������� ���� (����)
        Gizmos.color = Color.blue;
        Vector3 worldSmoothInput = transform.TransformDirection(_smoothedMoveInput);
        Gizmos.DrawRay(transform.position, worldSmoothInput * 1.5f);

        // ������������ ���� (������, ������)
        Gizmos.color = Color.yellow;
        Vector3 worldRawInput = transform.TransformDirection(_moveInput);
        Gizmos.DrawRay(transform.position, worldRawInput * 1f);

        // �������� ����
        Vector3 rayStart = legs != null ? legs.transform.position : transform.position;
        Gizmos.DrawRay(rayStart, rayStart + Vector3.down * groundRayLength);
    }
}