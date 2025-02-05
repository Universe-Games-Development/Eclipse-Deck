using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using UnityEngine;

public class CardAnimator : MonoBehaviour {
    [SerializeField] private LevitationData levitationData;
    [SerializeField] private RectTransform body; // Inner body for card animation

    private bool isHovered;
    private Tween hoveringTween;
    private CardUI cardUI;
    private int originalSiblingIndex;
    private Vector3 originalPosition;
    private Vector3 originalScale;

    public Action OnReachedOrigin;

    private void Awake() {
        cardUI = GetComponent<CardUI>();
        cardUI.OnCardClicked += ShrinkClick;

        originalPosition = body.localPosition;
        originalScale = body.localScale;
        originalSiblingIndex = transform.GetSiblingIndex();
    }

    private void Update() {
        if (Camera.main != null && body != null) {
            Vector3 directionToCamera = body.position - Camera.main.transform.position;
            directionToCamera.z = 0;
            body.rotation = Quaternion.LookRotation(directionToCamera);
        }
    }

    public void ToggleHover(bool isEnable) {
        if (isHovered == isEnable) return;
        isHovered = isEnable;

        if (isHovered) {
            StartLifting();
        } else {
            StopLifting();
        }
    }

    private void StartLifting() {
        if (body == null) {
            Debug.LogError("Body transform is null. Levitation cannot start.");
            return;
        }

        hoveringTween?.Kill();
        transform.SetSiblingIndex(transform.parent.childCount - 1);

        hoveringTween = body.DOMoveY(transform.position.y + levitationData.liftHeight, levitationData.liftDuration)
            .SetEase(Ease.InOutSine)
            .OnComplete(() => {
                if (isHovered) {
                    ContinuousHovering();
                }
            });
    }

    private void ContinuousHovering() {
        hoveringTween?.Kill();
        hoveringTween = body.DOMoveY(transform.position.y + levitationData.liftHeight + levitationData.levitationRange, levitationData.levitationSpeed)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    private void StopLifting() {
        transform.SetSiblingIndex(originalSiblingIndex);
        hoveringTween?.Kill();

        hoveringTween = body.DOMoveY(originalPosition.y, levitationData.dropDuration)
            .SetEase(Ease.InOutSine);
    }

    public void FlyToOrigin() {
        Vector3 randomSpawnPosition = transform.position + new Vector3(
            UnityEngine.Random.insideUnitSphere.x,
            levitationData.spawnHeight,
            UnityEngine.Random.insideUnitSphere.z
        );

        body.localPosition = transform.InverseTransformPoint(randomSpawnPosition);
        body.DOLocalMove(originalPosition, levitationData.spawnDuration)
            .OnComplete(() => OnReachedOrigin?.Invoke());
    }

    public async UniTask FlyAwayWithCallback() {
        hoveringTween?.Kill();

        Vector3 randomFlyPosition = transform.position + new Vector3(
            UnityEngine.Random.insideUnitSphere.x,
            levitationData.flyHeight,
            UnityEngine.Random.insideUnitSphere.z
        );

        await body.DOMove(randomFlyPosition, levitationData.flyAwayDuration).AsyncWaitForCompletion();
    }

    public void ShrinkClick(CardUI cardUI) {
        body.DOScale(0.9f, 0.1f).OnComplete(() => body.DOScale(1f, 0.1f));
    }

    public void Reset() {
        hoveringTween?.Kill();
        body.localPosition = originalPosition;
        body.localScale = originalScale;
        isHovered = false;
        OnReachedOrigin = null;
    }

    private void OnDestroy() {
        hoveringTween?.Kill();
        cardUI.OnCardClicked -= ShrinkClick;
    }

}