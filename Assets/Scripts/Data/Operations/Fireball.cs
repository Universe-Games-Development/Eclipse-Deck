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

    // ��������� ��������
    public enum FireballState {
        Idle,       // ������ ������, ������ �� ������
        Flying,     // ����� � ����
        Exploding,  // ����������
        Destroyed   // ���������
    }

    // �������
    public event Action OnLaunched;
    public event Action OnHitTarget;
    public event Action OnExplosionComplete;
    public event Action OnDestroyed;

    // ��������� ��������
    public FireballState CurrentState { get; private set; } = FireballState.Idle;
    public Transform Target { get; private set; }

    // ��������� ����
    private Camera mainCamera;
    private bool isManualControl = false;

    private void Awake() {
        mainCamera = Camera.main;
        CurrentState = FireballState.Idle;

        // ��������� ��� ������� � ������
        StopAllEffects();
    }

    private void Update() {
        // �������� ����� - ���������� �����
        if (testMode) {
            HandleTestInput();
        }
    }

    #region Public Methods

    /// <summary>
    /// ��������� ������� � ����
    /// </summary>
    public async UniTask LaunchToTarget(Transform target) {
        if (CurrentState != FireballState.Idle) {
            Debug.LogWarning("Fireball can only be launched from Idle state!");
            return;
        }

        Target = target;
        await ChangeState(FireballState.Flying);

        // ����� � ����
        await FlyToTarget();

        // ������������� ���������� ��� ����������
        await Explode();
    }

    /// <summary>
    /// ������������� �������� �������
    /// </summary>
    public async UniTask Explode() {
        if (CurrentState != FireballState.Flying && CurrentState != FireballState.Idle) {
            return;
        }

        await ChangeState(FireballState.Exploding);
        await PlayExplosion();
        await ChangeState(FireballState.Destroyed);

        // ���������� ������
        DestroyFireball();
    }

    /// <summary>
    /// �������� �������� ����� ���������� �����
    /// </summary>
    public void EnableTestMode() {
        testMode = true;
        isManualControl = true;

        // �������� ������� ������ ��� �����
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

        // ����� �� ����������� ���������
        await ExitState(previousState);

        // ���� � ����� ���������
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

        // ���� �������
        PlaySound(launchSound);

        // ����������� ���� ������
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

        // ������������� ����������� ����
        if (audioSource != null && audioSource.isPlaying) {
            audioSource.Stop();
        }
    }

    private async UniTask StartExplosionEffects() {
        // ������������� ������� ������
        StopFlyingEffects();

        // ��������� �����
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

            // ������������ ������� � ������� ��������
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
            await UniTask.Delay(500); // ��������
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

        // �������� �� �����
        MoveToMouse();

        // ���� - �����
        //if (Input.GetMouseButtonDown(0)) {
        //    _ = Explode();
        //}

        //// ������ ���� - ����� (��� �����)
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
        // ���� ����� �� ���������� ��������
        Destroy(gameObject, 0.5f);
    }

    private void OnDestroy() {
        OnDestroyed?.Invoke();
    }

    #endregion

    #region Debug

    private void OnDrawGizmos() {
        // ���������� ��������� � ���������
        Gizmos.color = GetStateColor();
        Gizmos.DrawWireSphere(transform.position, 0.5f);

        // ����� � ����
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