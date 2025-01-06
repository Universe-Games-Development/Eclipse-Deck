using System.Collections;
using UnityEngine;
using Zenject;

public class StartGameHandler : MonoBehaviour {
    private CameraSplineMover cameraSplineMover;
    private Animator uiAnimator;
    [SerializeField] private AnimationClip monitorMoveUp;

    private LevelManager levelManager;
    [Inject]
    public void Contruct(LevelManager levelManager) {
        this.levelManager = levelManager;
    }

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
        // �������� �������
        uiAnimator.SetTrigger("Lift");

        // ������ ���������� �������
        yield return new WaitForSeconds(monitorMoveUp.length);

        // �������� ��� ������ ���� ���������� �������
        cameraSplineMover.StartCameraMovement();
    }

    private void OnCameraMovementComplete() {
        levelManager.StartGame();
    }
}
