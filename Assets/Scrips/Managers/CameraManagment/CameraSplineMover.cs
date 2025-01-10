using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class CameraSplineMover : MonoBehaviour {
    public float duration = 5f;       // Тривалість руху
    public bool StartOnAwake = false; // Чи починати рух при запуску скрипта

    [SerializeField] private CinemachineSplineDolly dolly;
    [SerializeField] private float delayToMove;
    [SerializeField] private float endTime = 0f;

    public Action OnMovementStart;    // Делегат для початку руху
    public Action OnMovementComplete; // Делегат для завершення руху

    private void Awake() {
        if (StartOnAwake) {
            StartCameraMovement();
        }
    }

    public void StartCameraMovement() {
        if (dolly == null) {
            Debug.LogError("SplineComponent не встановлений!");
            return;
        }

        OnMovementStart?.Invoke(); // Викликаємо дію перед початком руху

        StartCoroutine(StartCoroutineWithDelay(dolly, delayToMove));
    }

    private IEnumerator StartCoroutineWithDelay(CinemachineSplineDolly dolly, float delay) {
        yield return new WaitForSeconds(delay);
        StartCoroutine(MoveCamera(dolly));
    }

    private IEnumerator MoveCamera(CinemachineSplineDolly dolly) {
        float elapsedTime = 0f;

        while (elapsedTime < duration) {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / duration);

            // Рух камери по сплайну
            dolly.CameraPosition = progress;

            yield return null;
        }

        // Додатковий час перед завершенням
        yield return new WaitForSeconds(endTime);

        // Викликаємо дію після завершення руху
        OnMovementComplete?.Invoke();
    }
}
