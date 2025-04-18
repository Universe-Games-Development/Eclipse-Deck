using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Splines;
using Zenject;

public class SplineMover : MonoBehaviour {
    [SerializeField] private SplineContainer splinePath;
    [SerializeField] private float duration = 2f;
    [SerializeField] private bool alignToDirection = true;
    [Inject] AnimationsDebugSettings animationDebugSettings;
    public void SetSpline(SplineContainer newSpline) {
        splinePath = newSpline;
    }

    public void SetDuration(float newDuration) {
        duration = newDuration;
    }

    public async UniTask MoveAlongSpline(Transform objectToMove) {
        if (animationDebugSettings.SkipAllAnimations) {
            objectToMove.position = splinePath.EvaluatePosition(1);
            return;
        }
        if (splinePath == null || objectToMove == null) {
            Debug.LogWarning("Spline path or target object is not set");
            return;
        }

        float t = 0;
        float pathLength = splinePath.CalculateLength();
        float startTime = Time.time;

        // ��������� ��������� �������
        objectToMove.position = splinePath.EvaluatePosition(0);

        while (t < 1 && splinePath != null) {
            // ��������� ������� ������� �� �������
            Vector3 position = splinePath.EvaluatePosition(t);
            objectToMove.position = position;

            // ������� ������� � ����������� ��������
            if (alignToDirection && t < 0.99f) {
                Vector3 nextPosition = splinePath.EvaluatePosition(t + 0.01f);
                Vector3 direction = (nextPosition - position).normalized;
                if (direction != Vector3.zero) {
                    objectToMove.rotation = Quaternion.LookRotation(direction);
                }
            }

            // �������� ������������ �� 0 �� 1 �� �������� �����
            t = (Time.time - startTime) / duration;

            await UniTask.Yield();
        }

        // ��������� ��������� �������
        if (splinePath)
        objectToMove.position = splinePath.EvaluatePosition(1);
    }
}