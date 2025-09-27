using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public class Fireball : MonoBehaviour {
    [Header("Movement")]
    public float speed = 10f;

    [Header("Particle Systems")]
    [SerializeField] private ParticleSystem fireParticles;
    [SerializeField] private ParticleSystem smokeParticles;
    [SerializeField] private ParticleSystem explosionParticles;
    [SerializeField] private ParticleSystem trailParticles;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip launchSound;
    [SerializeField] private AudioClip flyingSound;
    [SerializeField] private AudioClip explosionSound;

    [Header("Debug/Test")]
    [SerializeField] private bool testMode = false;
    [SerializeField] private BoardInputManager boardInputManager;
    [SerializeField] MovementComponent movementComponent;

    // Состояния файрбола
    public enum FireballState {
        Idle,       // Только создан, ничего не делает
        Flying,     // Летит к цели
        Exploding,  // Взрывается
        Destroyed   // Уничтожен
    }

    // События
    public event Action OnLaunched;
    public event Action OnHitTarget;
    public event Action OnExplosionComplete;
    public event Action OnDestroyed;

    // Публичные свойства
    public FireballState CurrentState { get; private set; } = FireballState.Idle;
    public Transform Target { get; private set; }

    // Приватные поля
    private Camera mainCamera;
    private bool isManualControl = false;

    private void Awake() {
        mainCamera = Camera.main;
        CurrentState = FireballState.Idle;

        // Выключаем все эффекты в начале
        StopAllEffects();
    }

    private void Update() {
        // Тестовый режим - управление мышью
        if (testMode) {
            HandleTestInput();
        }
    }

    #region Public Methods

    /// <summary>
    /// Запускает файрбол к цели
    /// </summary>
    public async UniTask LaunchToTarget(Transform target) {
        if (CurrentState != FireballState.Idle) {
            Debug.LogWarning("Fireball can only be launched from Idle state!");
            return;
        }

        Target = target;
        await ChangeState(FireballState.Flying);

        // Летим к цели
        await FlyToTarget();

        // Автоматически взрываемся при достижении
        await Explode();
    }

    /// <summary>
    /// Принудительно взрывает файрбол
    /// </summary>
    public async UniTask Explode() {
        if (CurrentState != FireballState.Flying && CurrentState != FireballState.Idle) {
            return;
        }

        await ChangeState(FireballState.Exploding);
        await PlayExplosion();
        await ChangeState(FireballState.Destroyed);

        // Уничтожаем объект
        DestroyFireball();
    }

    /// <summary>
    /// Включает тестовый режим управления мышью
    /// </summary>
    public void EnableTestMode() {
        testMode = true;
        isManualControl = true;

        // Включаем эффекты полета для теста
        if (CurrentState == FireballState.Idle) {
            _ = ChangeState(FireballState.Flying);
        }
    }

    #endregion

    #region State Management

    private async UniTask ChangeState(FireballState newState) {
        var previousState = CurrentState;
        CurrentState = newState;

        //Debug.Log($"Fireball state changed: {previousState} > {newState}");

        // Выход из предыдущего состояния
        await ExitState(previousState);

        // Вход в новое состояние
        await EnterState(newState);
    }

    private async UniTask EnterState(FireballState state) {
        switch (state) {
            case FireballState.Idle:
                StopAllEffects();
                break;

            case FireballState.Flying:
                await StartFlyingEffects();
                OnLaunched?.Invoke();
                break;

            case FireballState.Exploding:
                await StartExplosionEffects();
                OnHitTarget?.Invoke();
                break;

            case FireballState.Destroyed:
                StopAllEffects();
                OnDestroyed?.Invoke();
                break;
        }
    }

    private async UniTask ExitState(FireballState state) {
        switch (state) {
            case FireballState.Flying:
                StopFlyingEffects();
                break;

            case FireballState.Exploding:
                OnExplosionComplete?.Invoke();
                break;
        }

        await UniTask.Yield();
    }

    #endregion

    #region Effects Management

    private async UniTask StartFlyingEffects() {
        if (fireParticles != null) fireParticles.Play();
        if (smokeParticles != null) smokeParticles.Play();
        if (trailParticles != null) trailParticles.Play();

        // Звук запуска
        PlaySound(launchSound);

        // Зацикленный звук полета
        if (flyingSound != null && audioSource != null) {
            audioSource.clip = flyingSound;
            audioSource.loop = true;
            audioSource.Play();
        }

        await UniTask.Yield();
    }

    private void StopFlyingEffects() {
        if (fireParticles != null) fireParticles.Stop();
        if (smokeParticles != null) smokeParticles.Stop();
        if (trailParticles != null) trailParticles.Stop();

        // Останавливаем зацикленный звук
        if (audioSource != null && audioSource.isPlaying) {
            audioSource.Stop();
        }
    }

    private async UniTask StartExplosionEffects() {
        // Останавливаем эффекты полета
        StopFlyingEffects();

        // Запускаем взрыв
        if (explosionParticles != null) explosionParticles.Play();
        PlaySound(explosionSound);

        await UniTask.Yield();
    }

    private void StopAllEffects() {
        if (fireParticles != null) fireParticles.Stop();
        if (smokeParticles != null) smokeParticles.Stop();
        if (trailParticles != null) trailParticles.Stop();
        if (explosionParticles != null) explosionParticles.Stop();

        if (audioSource != null) {
            audioSource.Stop();
        }
    }

    private void PlaySound(AudioClip clip) {
        if (clip != null && audioSource != null) {
            audioSource.PlayOneShot(clip);
        }
    }

    #endregion

    #region Movement

    private async UniTask FlyToTarget() {
        if (Target == null) {
            Debug.LogError("No target set for fireball!");
            return;
        }

        while (Vector3.Distance(transform.position, Target.position) > 0.1f &&
               CurrentState == FireballState.Flying &&
               !isManualControl) {
            transform.position = Vector3.MoveTowards(
                transform.position,
                Target.position,
                speed * Time.deltaTime
            );

            // Поворачиваем файрбол в сторону движения
            Vector3 direction = (Target.position - transform.position).normalized;
            if (direction != Vector3.zero) {
                transform.rotation = Quaternion.LookRotation(direction);
            }

            await UniTask.Yield();
        }
    }

    #endregion

    #region Explosion

    private async UniTask PlayExplosion() {
        if (explosionParticles == null) {
            await UniTask.Delay(500); // Заглушка
            return;
        }

        float explosionDuration = explosionParticles.main.duration + explosionParticles.main.startLifetime.constantMax;
        await UniTask.Delay((int)(explosionDuration * 1000));
    }

    #endregion

    #region Test Mode

    private void HandleTestInput() {
        if (CurrentState == FireballState.Destroyed)
            return;

        // Движение за мышью
        MoveToMouse();

        // Клик - взрыв
        //if (Input.GetMouseButtonDown(0)) {
        //    _ = Explode();
        //}

        //// Правый клик - сброс (для теста)
        //if (Input.GetMouseButtonDown(1)) {
        //    ResetFireball();
        //}
    }

    private void MoveToMouse() {
        if (mainCamera == null)
            return;

        if (boardInputManager == null || !boardInputManager.TryGetCursorPosition(out Vector3 worldPos)) {
            return;
        }

        Vector3 newWorldPosition = new Vector3(worldPos.x, worldPos.y + boardInputManager.GetBoardHeightOffset(), worldPos.z);

        movementComponent.UpdateContinuousTarget(newWorldPosition);
    }

    private void ResetFireball() {
        StopAllCoroutines();
        CurrentState = FireballState.Idle;
        StopAllEffects();
        Target = null;
        isManualControl = false;

        Debug.Log("Fireball reset to Idle state");
    }

    #endregion

    #region Cleanup

    private void DestroyFireball() {
        // Даем время на завершение эффектов
        Destroy(gameObject, 0.5f);
    }

    private void OnDestroy() {
        OnDestroyed?.Invoke();
    }

    #endregion

    #region Debug

    private void OnDrawGizmos() {
        // Показываем состояние в редакторе
        Gizmos.color = GetStateColor();
        Gizmos.DrawWireSphere(transform.position, 0.5f);

        // Линия к цели
        if (Target != null && CurrentState == FireballState.Flying) {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, Target.position);
        }
    }

    private Color GetStateColor() {
        return CurrentState switch {
            FireballState.Idle => Color.gray,
            FireballState.Flying => Color.yellow,
            FireballState.Exploding => Color.red,
            FireballState.Destroyed => Color.black,
            _ => Color.white
        };
    }

    #endregion
}