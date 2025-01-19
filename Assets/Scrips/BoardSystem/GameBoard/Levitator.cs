using DG.Tweening;
using System;
using UnityEngine;

public class Levitator : MonoBehaviour {
    public Action OnFall;

    [Header("Levitation")]
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


    [SerializeField] private bool _isLevitating = false;

    private Tween levitationTween;

    [SerializeField] private float liftHeight = 1f;
    [SerializeField] private float levitationRange = 0.2f;
    [SerializeField] private float levitationSpeed = 1f;
    [SerializeField] private float dropDuration = 0.5f;

    [SerializeField] private float spawnHeight = 10f;
    [SerializeField] private float flyHeight = 10f;

    private Vector3 initialPosition;

    // After spawn animation
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
            levitationTween.Play();
        } else {
            levitationTween = transform.DOMoveY(initialPosition.y + liftHeight, 0.5f).OnComplete(() => {
                if (_isLevitating) {
                    ContinuousLevitation();
                }
            });
        }
    }


    private void ContinuousLevitation() {
        if (levitationTween != null && levitationTween.IsPlaying()) {
            return;
        } else {
            levitationTween = transform.DOMoveY(initialPosition.y + liftHeight + levitationRange, levitationSpeed)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }
    }


    private void StopLevitation() {
        if (levitationTween != null && levitationTween.IsPlaying()) {
            levitationTween.Pause(); // Зупиняємо твін
            transform.DOMoveY(initialPosition.y, dropDuration).OnComplete(() => {
                Debug.Log("Levitation stopped.");
            });
        }
    }


    public void FlyToInitialPosition() {
        // Визначення рандомної точки зверху initialPosition
        Vector3 randomSpawnPosition = initialPosition + new Vector3(UnityEngine.Random.insideUnitSphere.x, spawnHeight, UnityEngine.Random.insideUnitSphere.z);
        transform.position = randomSpawnPosition;

        transform.DOMove(initialPosition, dropDuration).OnComplete(() => {
            UpdateInitialPosition(transform.position);
            OnFall?.Invoke();
        });
    }

    public void FlyAway() {
        // Визначення рандомної точки вгору від initialPosition
        Vector3 randomFlyPosition = initialPosition + new Vector3(UnityEngine.Random.insideUnitSphere.x, flyHeight, UnityEngine.Random.insideUnitSphere.z);

        // Літати до випадкової позиції вгору
        transform.DOMove(randomFlyPosition, dropDuration).OnComplete(() => {
            Debug.Log("Flew away to random position");
        });
    }

    private void OnDestroy() {
        if (levitationTween != null) {
            levitationTween.Kill(); // Остаточно знищуємо твін
        }
    }

}
