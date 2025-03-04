using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Threading;
using UnityEngine;

public class CreatureAnimator : MonoBehaviour {
    public float moveDuration = 0.5f;
    public Ease moveEase = Ease.InOutQuad;
    public float spawnHeight = 1.5f;

    private CancellationTokenSource animationCTS;

    public async UniTask SpawnOnField(Transform targetPoint) {
        if (targetPoint == null) return;

        transform.SetParent(targetPoint);
        transform.localPosition = new Vector3(0, spawnHeight, 0);
        await MoveToPoint(Vector3.zero);
    }

    public async UniTask MoveToField(Transform targetPoint) {
        if (targetPoint == null) return;
        transform.SetParent(targetPoint);
        await MoveToPoint(Vector3.zero);
    }

    private async UniTask MoveToPoint(Vector3 targetPosition) {
        CancelCurrentAnimation();
        animationCTS = new CancellationTokenSource();

        try {
            await transform.DOLocalMove(targetPosition, moveDuration)
                .SetEase(moveEase)
                .AsyncWaitForCompletion()
                .AsUniTask()
                .AttachExternalCancellation(animationCTS.Token);
        } catch (OperationCanceledException) {
            Debug.Log("Creature movement canceled");
        } finally {
            transform.localPosition = targetPosition;
        }
    }

    public async UniTask InterruptedMove(Transform invalidPoint) {
        Vector3 initialPosition = transform.position;
        Vector3 halfWayPoint = Vector3.Lerp(initialPosition, invalidPoint.position, 0.5f);

        await transform.DOMove(halfWayPoint, 0.15f).SetEase(Ease.InQuad).AsyncWaitForCompletion();
        // Collision imitation animations
        await UniTask.WhenAll(
             transform.DOShakeScale(0.4f, 3, 5, 90).SetRelative(true).AsyncWaitForCompletion().AsUniTask(),
             transform.DOMove(initialPosition, 0.15f).SetEase(Ease.InQuad).AsyncWaitForCompletion().AsUniTask()
        );
    }


    public void ResetView() {
        CancelCurrentAnimation();
        transform.localPosition = Vector3.zero;
    }

    private void CancelCurrentAnimation() {
        animationCTS?.Cancel();
        animationCTS?.Dispose();
        animationCTS = null;
    }
}
