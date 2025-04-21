using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Splines;
using Zenject;

public class SplineMover : MonoBehaviour {
    [SerializeField] private float duration = 2f;
    [SerializeField] private bool alignToDirection = true;
   

    public void SetDuration(float newDuration) {
        duration = newDuration;
    }

    public async UniTask MoveAlongSpline(Transform objectToMove, SplineContainer splinePath, bool skip) {
        if (skip) {
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

        // Установка начальной позиции
        objectToMove.position = splinePath.EvaluatePosition(0);

        while (t < 1 && splinePath != null) {
            // Вычисляем текущую позицию на сплайне
            Vector3 position = splinePath.EvaluatePosition(t);
            objectToMove.position = position;

            // Поворот объекта в направлении движения
            if (alignToDirection && t < 0.99f) {
                Vector3 nextPosition = splinePath.EvaluatePosition(t + 0.01f);
                Vector3 direction = (nextPosition - position).normalized;
                if (direction != Vector3.zero) {
                    objectToMove.rotation = Quaternion.LookRotation(direction);
                }
            }

            // Линейная интерполяция от 0 до 1 за заданное время
            t = (Time.time - startTime) / duration;

            await UniTask.Yield();
        }

        // Финальная установка позиции
        if (splinePath)
        objectToMove.position = splinePath.EvaluatePosition(1);
    }
}