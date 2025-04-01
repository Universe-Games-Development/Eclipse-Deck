using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class CameraSplineMover : MonoBehaviour {
    public float duration = 5f;

    [SerializeField] private CinemachineSplineDolly dolly;
    [SerializeField] private float delayToMove;
    [SerializeField] private float endTime = 0f;

    public Action OnMovementStart;
    public Action OnMovementComplete;

    public void StartCameraMovement() {
        if (dolly == null) {
            Debug.LogError("SplineComponent не встановлений!");
            return;
        }

        OnMovementStart?.Invoke();

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
            dolly.CameraPosition = progress;

            yield return null;
        }

        yield return new WaitForSeconds(endTime);
        OnMovementComplete?.Invoke();
    }
}
