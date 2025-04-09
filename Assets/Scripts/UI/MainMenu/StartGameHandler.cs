using System.Collections;
using UnityEngine;
using Zenject;

public class StartGameHandler : MonoBehaviour {
    [SerializeField] private CameraSplineMover cameraSplineMover;
    [SerializeField] private Animator monitorAnimator;
    [SerializeField] private AnimationClip monitorMoveUp;

    private GameManager gameManager;
    [Inject]
    public void Contruct(GameManager gameManager) {
        this.gameManager = gameManager;
    }

    private void Awake() {
        if (cameraSplineMover != null) {
            cameraSplineMover.OnMovementComplete += OnCameraMovementComplete;
        }
    }

    public void StartGame() {
        gameManager.StartNewGame();
        //StartCoroutine(StartGameCoroutine());
    }

    private IEnumerator StartGameCoroutine() {
        // �������� �������
        monitorAnimator.SetTrigger("Lift");

        yield return new WaitForSeconds(monitorMoveUp.length);

        //// �������� ��� ������ ���� ���������� �������
        //cameraSplineMover.StartCameraMovement();
    }

    private void OnCameraMovementComplete() {
        gameManager.StartNewGame();
    }

    private void OnDestroy() {
        if (cameraSplineMover != null) {
            cameraSplineMover.OnMovementComplete -= OnCameraMovementComplete;
        }
    }
}
