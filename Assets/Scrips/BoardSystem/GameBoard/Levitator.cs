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
    [SerializeField] private Transform body;

    public void ToggleLevitation(bool isEnable) {
        if (isLevitating != isEnable) {
            isLevitating = isEnable;
        }
    }

    private void StartLevitation() {
        if (body == null) {
            Debug.LogError("Body transform is null. Levitation cannot start.");
            return;
        }

        if (levitationTween != null && levitationTween.IsActive()) {
            levitationTween.Kill();
        }

        levitationTween = body.DOMoveY(transform.position.y + levitationData.liftHeight, levitationData.liftDuration)
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

        levitationTween = body.DOMoveY(transform.position.y + levitationData.liftHeight + levitationData.levitationRange, levitationData.levitationSpeed)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    private void StopLevitation() {
        if (levitationTween != null && levitationTween.IsActive()) {
            levitationTween.Kill();
        }

        // Перевіряємо чи позиція на землі (висота y повинна бути близька до початкової)
        if (Mathf.Abs(transform.position.y - body.position.y) > 0.1f) { // 0.1f - допустима похибка
            levitationTween = body.DOMoveY(transform.position.y, levitationData.dropDuration)
                .SetEase(Ease.InOutSine)
                .OnComplete(() => {
                    Debug.Log("Levitation stopped and position corrected.");
                });
        } else {
            // Якщо позиція вже правильна, просто зупиняємо левітацію
            levitationTween = body.DOMoveY(transform.position.y, levitationData.dropDuration)
                .SetEase(Ease.InOutSine)
                .OnComplete(() => {
                    Debug.Log("Levitation stopped.");
                });
        }
    }


    public void FlyToInitialPosition() {
        // Генеруємо випадкову стартову позицію в світових координатах
        Vector3 randomSpawnPosition = transform.position + new Vector3(
            UnityEngine.Random.insideUnitSphere.x,
            levitationData.spawnHeight,
            UnityEngine.Random.insideUnitSphere.z
        );

        // Перетворюємо в локальні координати відносно батьківського об'єкта body
        Vector3 localSpawnPosition = transform.InverseTransformPoint(randomSpawnPosition);

        // Встановлюємо початкову позицію body
        body.localPosition = localSpawnPosition;

        // Анімуємо body до початкової позиції transform.position у локальних координатах
        Vector3 targetLocalPosition = transform.InverseTransformPoint(transform.position);

        body.DOLocalMove(targetLocalPosition, levitationData.spawnDuration).OnComplete(() => {
            OnFall?.Invoke();
        });
    }


    public void FlyAwayWithCallback(Action onComplete) {
        if (levitationTween != null) {
            levitationTween.Kill();
        }

        Vector3 randomFlyPosition = transform.position + new Vector3(
            UnityEngine.Random.insideUnitSphere.x,
            levitationData.flyHeight,
            UnityEngine.Random.insideUnitSphere.z
        );

        body.DOMove(randomFlyPosition, levitationData.flyAwayDuration)
            .OnComplete(() => {
                onComplete?.Invoke(); // Викликаємо колбек після завершення анімації
            });
    }


    public void Reset() {
        if (levitationTween != null && levitationTween.IsActive()) {
            levitationTween.Kill();
            levitationTween = null;
        }

        if (body != null) {
            body.localPosition = Vector3.zero;
        }

        _isLevitating = false;
        OnFall = null;
    }


    private void OnDestroy() {
        if (levitationTween != null) {
            levitationTween.Kill();
            levitationTween = null;
        }
    }
}