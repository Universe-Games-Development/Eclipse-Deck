using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Splines;

public class OpponentView : MonoBehaviour {
    public event Action OnSeatTaken;

    [SerializeField] private Animator animator;
    [SerializeField] private bool useDoTween = false;
    [SerializeField] protected SplineMover splineMover;

    private IMover mover;
    // Saved data
    private Vector3 previousPosition;
    private Transform previousParent;
    private CancellationTokenSource _cts;

    private void Awake() {
        mover = useDoTween ? new DoTweenMover() : new ObjectMover();
        _cts = new CancellationTokenSource();
    }

    private void OnDestroy() {
        _cts?.Cancel();
        _cts?.Dispose();
    }

    public virtual async UniTask MoveToPositionAsync(Vector3 target, float duration = 1) {
        try {
            await mover.MoveAsync(transform, target, duration).AttachExternalCancellation(_cts.Token);
        } catch (OperationCanceledException) {
            // Task was canceled - safely ignored
        }
    }

    public async UniTask TookSeat(BoardSeat seat) {
        previousPosition = transform.position;
        previousParent = transform.parent;
        transform.SetParent(seat.transform);
        await MoveToPositionAsync(seat.transform.position, 0.5f);
        OnSeatTaken?.Invoke(); // Now this event is properly invoked
    }

    public async UniTask ClearSeat() {
        transform.SetParent(previousParent);
        await MoveToPositionAsync(previousPosition, 0.5f);
    }

    public virtual async UniTask EnterRoom(SplineContainer splineContainer) {
        await MoveAlongSpline(splineContainer);
    }

    public virtual async UniTask ExitRoom(SplineContainer splineContainer) {
        await MoveAlongSpline(splineContainer);
    }

    public async UniTask MoveAlongSpline(SplineContainer splineContainer) {
        if (animator != null) {
            animator.SetTrigger("Appear");
        }

        if (splineMover != null) {
            try {
                await splineMover.MoveAlongSpline(transform, splineContainer, false).AttachExternalCancellation(_cts.Token);
            } catch (OperationCanceledException) {
                // Ignored if canceled
            }
        }
    }
}

