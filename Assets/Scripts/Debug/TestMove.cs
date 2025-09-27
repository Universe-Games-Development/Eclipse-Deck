using DG.Tweening;
using UnityEngine;

public class TestMove : MonoBehaviour {
    [SerializeField] private RectTransform target;
    [SerializeField] private RectTransform self;

    private void Awake() {
        self.DOMove(target.position, 0.8f).SetEase(Ease.InOutSine);
    }
}
