// IMover.cs - спільний інтерфейс для усіх муверів
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;




public class OpponentView : MonoBehaviour {
    [SerializeField]
    private bool useDoTween = false;

    private IMover mover;

    // Saved data
    private Vector3 previousPosition;
    private Transform previousParent;

    private void Awake() {
        // Ініціалізуємо відповідний мувер залежно від налаштувань
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
}