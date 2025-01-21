using DG.Tweening;
using System;
using UnityEngine;

public class Levitator : MonoBehaviour {
    public Action OnFall;

    [Header("Levitation Data")]
    [SerializeField] private LevitationData levitationData;

    private bool isLevitating {
        get { return _isLevitating; }
        set {
            _isLevitating = value;
            if (_isLevitating) {
                StartLevitation();
            } else {
                StopLevitation();
            }
        }
    }

    private bool _isLevitating = false;
    private Tween levitationTween;
    private Vector3 initialPosition;
    [SerializeField] private Transform body;

    public void UpdateInitialPosition(Vector3 vector) {
        initialPosition = vector;
    }

    public void ToggleLevitation(bool isEnable) {
        if (isLevitating != isEnable) {
            isLevitating = isEnable;
        }
    }

    private void StartLevitation() {
        if (levitationTween != null && levitationTween.IsActive()) {
            levitationTween.Kill();
        }

        levitationTween = body.DOMoveY(initialPosition.y + levitationData.liftHeight, levitationData.liftDuration)
            .SetEase(Ease.InOutSine)
            .OnComplete(() => {
                if (_isLevitating) {
                    ContinuousLevitation();
                }
            });
    }

    private void ContinuousLevitation() {
        if (levitationTween != null && levitationTween.IsPlaying()) {
            levitationTween.Kill();
        }

        levitationTween = body.DOMoveY(initialPosition.y + levitationData.liftHeight + levitationData.levitationRange, levitationData.levitationSpeed)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    private void StopLevitation() {
        if (levitationTween != null && levitationTween.IsActive()) {
            levitationTween.Kill();
        }

        levitationTween = body.DOMoveY(initialPosition.y, levitationData.dropDuration)
            .SetEase(Ease.InOutSine)
            .OnComplete(() => {
                Debug.Log("Levitation stopped.");
            });
    }

    public void FlyToInitialPosition() {
        Vector3 randomSpawnPosition = initialPosition + new Vector3(UnityEngine.Random.insideUnitSphere.x, levitationData.spawnHeight, UnityEngine.Random.insideUnitSphere.z);
        body.position = randomSpawnPosition;

        body.DOMove(initialPosition, levitationData.spawnDuration).OnComplete(() => {
            UpdateInitialPosition(transform.position);
            OnFall?.Invoke();
        });
    }

    public void FlyAway() {
        if (levitationTween != null) {
            levitationTween.Kill();
        } 
        Vector3 randomFlyPosition = initialPosition + new Vector3(UnityEngine.Random.insideUnitSphere.x, levitationData.flyHeight, UnityEngine.Random.insideUnitSphere.z);

        body.DOMove(randomFlyPosition, levitationData.flyAwayDuration).OnComplete(() => {
            Debug.Log("Flew away to random position");
        });
    }

    public void Reset() {
        if (levitationTween != null && levitationTween.IsActive()) {
            levitationTween.Kill();
            levitationTween = null;
        }

        _isLevitating = false;
        OnFall = null;
    }


    private void OnDestroy() {
        if (levitationTween != null) {
            levitationTween.Kill();
            levitationTween = null;
        }
        body = null;
    }

}
