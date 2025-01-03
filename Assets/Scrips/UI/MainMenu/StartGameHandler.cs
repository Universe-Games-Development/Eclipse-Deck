using UnityEngine;
using System.Collections;

public class StartGameHandler : MonoBehaviour {
    private CameraSplineMover cameraSplineMover;
    private Animator uiAnimator;
    [SerializeField] private AnimationClip monitorMoveUp;

    private void Awake() {
        cameraSplineMover = GetComponent<CameraSplineMover>();

        if (cameraSplineMover != null) {
            cameraSplineMover.OnMovementComplete += OnCameraMovementComplete;
        }

        uiAnimator = GetComponent<Animator>();
    }

    private void OnDestroy() {
        if (cameraSplineMover != null) {
            cameraSplineMover.OnMovementComplete -= OnCameraMovementComplete;
        }
    }

    public void StartGame() {
        StartCoroutine(StartGameCoroutine());
    }

    private IEnumerator StartGameCoroutine() {
        // Виконуємо анімацію
        uiAnimator.SetTrigger("Lift");

        // Чекаємо завершення анімації
        yield return new WaitForSeconds(monitorMoveUp.length);

        // Починаємо рух камери після завершення анімації
        cameraSplineMover.StartCameraMovement();
    }

    private void OnCameraMovementComplete() {
        if (LevelManager.Instance != null) {
            LevelManager.Instance.StartGame();
        } else {
            Debug.LogError("LevelManager.Instance не знайдено!");
        }
    }
}
