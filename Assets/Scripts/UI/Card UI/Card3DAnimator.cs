using DG.Tweening;
using UnityEngine;

public class Card3DAnimator : MonoBehaviour {
    [SerializeField] private Transform target;

    [Header("Hover Animation")]
    [SerializeField] private float hoverHeight = 0.3f;
    [SerializeField] private float hoverZOffset = 0.3f;
    [SerializeField] private float hoverSpeedDuration = 0.2f;
    [SerializeField] private Ease hoverEase = Ease.OutQuad;

    [Header("Click Animation")]
    [SerializeField] private float clickScaleDown = 0.95f;
    [SerializeField] private float clickDuration = 0.1f;
    [SerializeField] private float clickReturnDuration = 0.15f;

    private Vector3 originalPosition;
    private Vector3 originalScale;
    private Quaternion originalRotation;

    public void Awake() {
        if (target == null) {
            Debug.LogWarning($"Target not set for animator at : {gameObject}");
            target = transform;
        }
        originalPosition = target.localPosition;
        originalScale = target.localScale;
        originalRotation = target.localRotation;
    }

    public void Hover(bool hovered) {
        if (hovered) {
            // Hover animation - lift the card up
            Sequence hoverSequence = DOTween.Sequence();
            hoverSequence
                .Join(target.DOLocalMoveY(originalPosition.y + hoverHeight, hoverSpeedDuration).SetEase(hoverEase))
                .Join(target.DOLocalMoveZ(originalPosition.z + hoverZOffset, hoverSpeedDuration))
                .Play();

        } else {
            target.DOLocalMove(originalPosition, hoverSpeedDuration).SetEase(hoverEase).Play();
        }
    }
    public void PlayClickAnimation() {
        // Quick scale down and up animation
        target.DOScale(originalScale * clickScaleDown, clickDuration).SetEase(Ease.InQuad)
            .OnComplete(() => {
                target.DOScale(originalScale, clickReturnDuration).SetEase(Ease.OutQuad);
            });
    }

    public void AnimateStatChange(GameObject statObject, int from, int to) {
        if (statObject == null) return;

        // Flash and scale the stat object
        Sequence statChangeSequence = DOTween.Sequence();

        // Scale up
        statChangeSequence.Append(statObject.transform.DOScale(Vector3.one * 1.5f, 0.2f).SetEase(Ease.OutQuad));

        // And back down
        statChangeSequence.Append(statObject.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.InQuad));

        // Change color based on whether stat increased or decreased
        if (to > from) {
            // Positive change - green flash
            FlashColor(statObject, Color.green);
        } else if (to < from) {
            // Negative change - red flash
            FlashColor(statObject, Color.red);
        }
    }

    private void FlashColor(GameObject obj, Color flashColor) {
        Debug.Log($"Flashing {obj.name} with color {flashColor}");
    }

    public void Reset() {
        DOTween.Kill(transform);
    }
}