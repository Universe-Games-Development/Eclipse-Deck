using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using UnityEngine;

public class Levitator : MonoBehaviour {
    public event Action OnFall;

    [Header("Levitation Data")]
    [SerializeField] private LevitationData levitationData;
    [SerializeField] private Transform body;

    private bool isLevitating = false;
    private Tween levitationTween;

    public void ToggleLevitation(bool enable) {
        if (isLevitating == enable) return;

        isLevitating = enable;
        if (isLevitating)
            StartLevitation();
        else
            StopLevitation();
    }

    private void StartLevitation() {
        if (body == null) {
            Debug.LogError("Body transform is null. Levitation cannot start.");
            return;
        }
        KillActiveTween();

        levitationTween = body.DOMoveY(transform.position.y + levitationData.liftHeight, levitationData.liftDuration)
            .SetEase(Ease.InOutSine)
            .OnComplete(() => {
                if (isLevitating)
                    ContinuousLevitation();
            });
    }

    private void ContinuousLevitation() {
        KillActiveTween();
        levitationTween = body.DOMoveY(
            transform.position.y + levitationData.liftHeight + levitationData.levitationRange,
            levitationData.levitationSpeed
        )
        .SetLoops(-1, LoopType.Yoyo)
        .SetEase(Ease.InOutSine);
    }
    private void StopLevitation() {
        KillActiveTween();

        float distanceToGround = Mathf.Abs(transform.position.y - body.position.y);

        levitationTween = body.DOMoveY(transform.position.y, levitationData.dropDuration)
            .SetEase(Ease.InOutSine);
    }

    public void FlyToInitialPosition() {
        KillActiveTween();

        // Generate random spawn position in world coordinates
        Vector3 randomSpawnPosition = transform.position + new Vector3(
            UnityEngine.Random.insideUnitSphere.x,
            levitationData.spawnHeight,
            UnityEngine.Random.insideUnitSphere.z
        );

        // Convert to local coordinates relative to parent object
        Vector3 localSpawnPosition = transform.InverseTransformPoint(randomSpawnPosition);

        // Set initial position
        body.localPosition = localSpawnPosition;

        // Animate to initial position
        Vector3 targetLocalPosition = transform.InverseTransformPoint(transform.position);

        levitationTween = body.DOLocalMove(targetLocalPosition, levitationData.spawnDuration)
            .OnComplete(() => OnFall?.Invoke());
    }

    public async UniTask FlyAwayWithCallback() {
        KillActiveTween();

        Vector3 randomFlyPosition = transform.position + new Vector3(
            UnityEngine.Random.insideUnitSphere.x,
            levitationData.flyHeight,
            UnityEngine.Random.insideUnitSphere.z
        );

        await body.DOMove(randomFlyPosition, levitationData.flyAwayDuration).AsyncWaitForCompletion();
    }

    public void Reset() {
        KillActiveTween();

        if (body != null)
            body.localPosition = Vector3.zero;

        isLevitating = false;
        OnFall = null;
    }

    private void KillActiveTween() {
        if (levitationTween != null && levitationTween.IsActive())
            levitationTween.Kill();
    }

    private void OnDestroy() {
        KillActiveTween();
        levitationTween = null;
    }
}