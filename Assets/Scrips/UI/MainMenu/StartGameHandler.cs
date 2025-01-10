using System.Collections;
using UnityEngine;
using Zenject;

public class StartGameHandler : MonoBehaviour {
    [SerializeField] private CameraSplineMover cameraSplineMover;
    [SerializeField] private Animator monitorAnimator;
    [SerializeField] private AnimationClip monitorMoveUp;

    private LevelManager levelManager;
    [Inject]
    public void Contruct(LevelManager levelManager) {
        this.levelManager = levelManager;
    }

    private void Awake() {
        if (cameraSplineMover != null) {
            cameraSplineMover.OnMovementComplete += OnCameraMovementComplete;
        }
    }

    public void StartGame() {
        StartCoroutine(StartGameCoroutine());
    }

    private IEnumerator StartGameCoroutine() {
        // Виконуємо анімацію
        monitorAnimator.SetTrigger("Lift");

        yield return new WaitForSeconds(monitorMoveUp.length);

        // Починаємо рух камери після завершення анімації
        cameraSplineMover.StartCameraMovement();
    }

    private void OnCameraMovementComplete() {
        levelManager.StartGame();
    }

    private void OnDestroy() {
        if (cameraSplineMover != null) {
            cameraSplineMover.OnMovementComplete -= OnCameraMovementComplete;
        }
    }
}
