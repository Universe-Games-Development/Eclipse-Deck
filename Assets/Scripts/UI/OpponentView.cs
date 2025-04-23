using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.Splines;
using Zenject;

public class OpponentView : MonoBehaviour {
    [SerializeField] private Animator animator;
    [SerializeField] private bool useDoTween = false;
    
    [SerializeField] protected SplineMover splineMover;
    private IMover mover;
    // Saved data
    private Vector3 previousPosition;
    private Transform previousParent;

    private void Awake() {
        mover = useDoTween ? new DoTweenMover() : new ObjectMover();
    }


    public virtual async UniTask MoveToPositionAsync(Vector3 target, float duration = 1) {
        await mover.MoveAsync(transform, target, duration);
    }

    internal async UniTask TookSeat(BoardSeat seat) {
        previousPosition = transform.position;
        previousParent = transform.parent;
        await MoveToPositionAsync(seat.transform.position, 0.5f);
        transform.SetParent(seat.transform);
    }

    public async UniTask ClearSeat() {
        await MoveToPositionAsync(previousPosition, 0.5f);
        transform.SetParent(previousParent);
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
            await splineMover.MoveAlongSpline(transform, splineContainer, false);
        } else {
            await UniTask.CompletedTask;
        }
    }
}
