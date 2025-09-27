using System;
using UnityEngine;

[RequireComponent(typeof(MovementComponent))]
public class CardTiltController : MonoBehaviour {
    [Header("Tilt Settings")]
    [SerializeField] private float forwardTiltSensitivity = 2f;
    [SerializeField] private float sideTiltSensitivity = 1.5f;
    [SerializeField] private float verticalTiltSensitivity = 0.8f;
    [SerializeField] private float maxTiltAngle = 25f;
    [SerializeField] private float tiltSmoothing = 8f;
    [SerializeField] private float velocityThreshold = 0.1f; // Нова змінна

    private Vector3 smoothedVelocity;
    private Quaternion baseRotation;
    private MovementComponent movementComponent;

    public bool isEnabled = false;

    private void Awake() {
        movementComponent = GetComponent<MovementComponent>();

        baseRotation = transform.rotation;
        smoothedVelocity = Vector3.zero;
    }

    private void Update() {
        if (movementComponent != null && isEnabled) {
            UpdateTilt(movementComponent.CurrentVelocity);
        }
    }

    public void UpdateTilt(Vector3 velocity) {
        // Згладжування швидкості
        smoothedVelocity = Vector3.Lerp(smoothedVelocity, velocity, Time.deltaTime * 10f);

        // Перевірка на мінімальну швидкість
        if (smoothedVelocity.magnitude < velocityThreshold) {
            // Плавне повернення до базового повороту
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                baseRotation,
                Time.deltaTime * tiltSmoothing
            );
            return;
        }

        // Обчислення нахилів
        float pitchFromVelocity = -smoothedVelocity.z * forwardTiltSensitivity;
        float rollFromVelocity = smoothedVelocity.x * sideTiltSensitivity;
        float pitchFromVertical = -smoothedVelocity.y * verticalTiltSensitivity;

        float totalPitch = Mathf.Clamp(pitchFromVelocity + pitchFromVertical, -maxTiltAngle, maxTiltAngle);
        float totalRoll = Mathf.Clamp(rollFromVelocity, -maxTiltAngle, maxTiltAngle);
        float yawFromVelocity = Mathf.Clamp(smoothedVelocity.x * 0.5f, -maxTiltAngle * 0.5f, maxTiltAngle * 0.5f);

        Vector3 targetEuler = new Vector3(totalPitch, yawFromVelocity, totalRoll);
        Quaternion targetRotation = baseRotation * Quaternion.Euler(targetEuler);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * tiltSmoothing
        );
    }

    public void ToggleTiling(bool enable) {
        isEnabled = enable;
        if (!isEnabled) {
            transform.rotation = baseRotation;
        }
    }
}