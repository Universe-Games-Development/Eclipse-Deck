using UnityEngine;

public class CardTiltController : MonoBehaviour {
    [Header("Tilt Settings")]
    [SerializeField] private float forwardTiltSensitivity = 2f;
    [SerializeField] private float sideTiltSensitivity = 1.5f;
    [SerializeField] private float verticalTiltSensitivity = 0.8f;
    [SerializeField] private float maxTiltAngle = 25f;
    [SerializeField] private float tiltSmoothing = 8f;
    [SerializeField] private float rotationDamping = 0.95f;

    private Vector3 smoothedVelocity;
    private Quaternion baseRotation;

    private void Awake() {
        baseRotation = transform.rotation;
        smoothedVelocity = Vector3.zero;
    }

    public void UpdateTilt(Vector3 velocity) {

        // Згладжування швидкості
        smoothedVelocity = Vector3.Lerp(smoothedVelocity, velocity, Time.deltaTime * 10f);

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

    public void ResetToBase() {
        StartCoroutine(SmoothResetRotation());
    }

    private System.Collections.IEnumerator SmoothResetRotation() {
        Quaternion startRotation = transform.rotation;
        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration) {
            float t = elapsed / duration;
            transform.rotation = Quaternion.Slerp(startRotation, baseRotation, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
}