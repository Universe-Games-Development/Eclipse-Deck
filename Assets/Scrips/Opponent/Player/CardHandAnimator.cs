using UnityEngine;
using DG.Tweening;

public class CardHandAnimator : MonoBehaviour {
    [SerializeField] private CardHandUIAnimationData animationData;
    [SerializeField] private RectTransform innerBody;
    private void Awake() {
        StartShakeAnimation();
        StartLiftUpAnimation();
    }

    private void StartLiftUpAnimation() {
        if (innerBody) {
            RectTransform rectTransform = innerBody.GetComponent<RectTransform>();

            if (rectTransform != null) {
                rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, -rectTransform.rect.height); // Start from below
                rectTransform.DOAnchorPos(Vector2.zero, 0.6f); // Move to original position
            } else {
                Debug.LogError("innerBody does not have a RectTransform!");
            }
        }
    }

    private void StartShakeAnimation() {
        transform.DOShakePosition(animationData.shakeDuration, animationData.shakeStrength, animationData.shakeVibration, animationData.shakeRandomness)
             .SetLoops(-1, LoopType.Restart);
    }
}