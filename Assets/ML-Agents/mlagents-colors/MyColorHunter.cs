using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class MyColorHunter : Agent {

    [SerializeField] private CharacterMover _characterMover;
    [SerializeField] private ColorGame colorGame;
    [SerializeField] private VisionComponent visionComponent;

    [Header("Rewards")]
    [SerializeField] private float stepPenalty = -0.001f;
    [SerializeField] private float idlePenalty = -0.002f;
    [SerializeField] private float correctReward = 1.0f;
    [SerializeField] private float wrongPenalty = -0.2f;
    [SerializeField] private float deathPenalty = -0.4f;
    [SerializeField] private float approachReward = 0.01f;

    [Header("Action Smoothing")]
    [Range(0f, 0.95f)]
    [SerializeField] private float actionSmoothing = 0.3f;
    private Vector3 _lastActions = Vector3.zero;

    [SerializeField] private ColorInfo targetInfo;
    [SerializeField] private Transform spawnPoint;
    private bool isResetting = false;
    [SerializeField] private float resetDelay = 0.1f;

    // Кешування для оптимізації (track distance for approach reward)
    private float _lastBestDistance = 1f;

    // Кеш rayHits для поточного кадру — щоб не виконувати PerformRaycasts кілька разів
    private IReadOnlyList<VisionComponent.RayHitInfo> _cachedRayHits = null;
    private int _cachedRayHitsFrame = -1;

    public override void Initialize() {
        if (visionComponent == null) {
            visionComponent = GetComponent<VisionComponent>();
        }
        if (colorGame != null) colorGame.OnGameTimeExpired += HandleGameTimeExpired;
    }

    private void HandleGameTimeExpired() {
        SetReward(deathPenalty);
        Debug.Log($"<color=red>Time expired! Death: {deathPenalty}</color>");
        EndEpisode();
    }

    public override void OnEpisodeBegin() {
        isResetting = true;

        if (_characterMover != null) _characterMover.StopMovement();

        // Телепортуємо агента
        transform.position = spawnPoint ? spawnPoint.position : Vector3.one + Vector3.up * 3f;
        transform.rotation = spawnPoint ? spawnPoint.rotation : Quaternion.identity;

        if (colorGame != null) {
            colorGame.ResetGame();
            colorGame.StartGame();
            targetInfo = colorGame.GetTargetColor();
        }

        // Скидання кешів/стану
        _lastBestDistance = 1f;
        _lastActions = Vector3.zero;
        _cachedRayHits = null;
        _cachedRayHitsFrame = -1;

        // Скидаємо прапорець через невеликий delay, щоб уникнути "подвійного" OnCollisionEnter
        StartCoroutine(ResetCollisionFlag());
    }

    private IEnumerator ResetCollisionFlag() {
        // Коротка затримка — дозволяє Unity фізиці "устаканитись"
        yield return new WaitForSeconds(resetDelay);
        isResetting = false;
    }

    public override void CollectObservations(VectorSensor sensor) {
        // Позиція (можна зменшити вимірності за потреби)
        sensor.AddObservation(transform.position);

        // Використовуємо єдиний метод, який робить raycasts і повертає найкращий об'єкт
        // Ми також додаємо спостереження з променів всередині ProceedRayHits
        ColorObject bestMatchFromRays = ProceedRayHits(sensor);

        if (bestMatchFromRays != null) {
            Vector3 directionToBest = (bestMatchFromRays.transform.position - transform.position).normalized;
            Vector3 localDir = transform.InverseTransformDirection(directionToBest);
            sensor.AddObservation(localDir.x);
            sensor.AddObservation(localDir.z);
        } else {
            sensor.AddObservation(0f);
            sensor.AddObservation(0f);
        }
    }

    /// <summary>
    /// Виконує raycasts через visionComponent і додає спостереження по променях.
    /// Повертає найкращий знайдений ColorObject (або null).
    /// Кешує результат на поточний кадр для повторного використання.
    /// </summary>
    private ColorObject ProceedRayHits(VectorSensor sensor) {
        // Кешування результату на кадр (щоб уникнути дублювання PerformRaycasts)
        if (_cachedRayHits == null || _cachedRayHitsFrame != Time.frameCount) {
            _cachedRayHits = visionComponent.PerformRaycasts();
            _cachedRayHitsFrame = Time.frameCount;
        }

        IReadOnlyList<VisionComponent.RayHitInfo> rayHits = _cachedRayHits;

        // Якщо промені не всі виконалися (неповні дані) - заповнюємо нулями
        if (rayHits == null || rayHits.Count < visionComponent.RayCount) {
            for (int i = 0; i < visionComponent.RayCount; i++) {
                sensor.AddObservation(0f); // similarity
                sensor.AddObservation(0f); // distance
            }
            return null;
        }

        ColorObject bestMatch = null;
        float bestSimilarity = 0f;

        // Перебираємо всі промені, додаємо до sensors
        foreach (var item in rayHits) {
            if (item.hasHit && item.hitObject != null && item.hitObject.CompareTag("Color Object")) {
                if (item.hitObject.TryGetComponent(out ColorObject colorObject) && colorObject != null) {
                    // Обчислюємо схожість кольору
                    float similarity = CalculateColorSimilarity(colorObject.ColorInfo.color, targetInfo.color);

                    // Зберігаємо спостереження
                    sensor.AddObservation(similarity);
                    sensor.AddObservation(item.normalizedDistance);

                    // Вибираємо кращий
                    if (similarity > bestSimilarity) {
                        bestSimilarity = similarity;
                        bestMatch = colorObject;
                    }
                    continue;
                }
            }

            // Якщо нічого не знайдено на промені
            sensor.AddObservation(0f); // similarity
            sensor.AddObservation(item.normalizedDistance);
        }

        return bestMatch;
    }

    private float CalculateColorSimilarity(Color color1, Color color2) {
        float dr = color1.r - color2.r;
        float dg = color1.g - color2.g;
        float db = color1.b - color2.b;
        float distance = Mathf.Sqrt(dr * dr + dg * dg + db * db);
        return 1f / (1f + distance);
    }

    public override void Heuristic(in ActionBuffers actionsOut) {
        var continuousActions = actionsOut.ContinuousActions;

        continuousActions[0] = Input.GetAxis("Vertical");
        continuousActions[1] = Input.GetAxis("Horizontal");

        if (Input.GetKey(KeyCode.Q)) {
            continuousActions[2] = -1f;
        } else if (Input.GetKey(KeyCode.E)) {
            continuousActions[2] = 1f;
        } else {
            continuousActions[2] = 0f;
        }
    }

    public override void OnActionReceived(ActionBuffers actions) {
        float rawForward = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        float rawRight = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);
        float rawTurn = Mathf.Clamp(actions.ContinuousActions[2], -1f, 1f);

        Vector3 currentActions = new Vector3(rawForward, rawRight, rawTurn);
        Vector3 smoothedActions = Vector3.Lerp(currentActions, _lastActions, actionSmoothing);
        _lastActions = smoothedActions;

        float forwardInput = smoothedActions.x;
        float rightInput = smoothedActions.y;
        float turnInput = smoothedActions.z;

        if (_characterMover != null) {
            _characterMover.SetMovementInput(forwardInput, rightInput);
            _characterMover.SetTurnInput(turnInput);
        }

        // Базовий штраф
        AddReward(stepPenalty);

        // Штраф за бездіяльність
        float actionMagnitude = Mathf.Abs(forwardInput) + Mathf.Abs(rightInput) + Mathf.Abs(turnInput);
        if (actionMagnitude < 0.15f) {
            AddReward(idlePenalty);
        }

        RewardForApproaching();
    }

    /// <summary>
    /// Нагорода за наближення: використовує той же cached rayHits / пошук найкращого об'єкта
    /// </summary>
    private void RewardForApproaching() {
        // Використовуємо кешовані результати PerformRaycasts (з Proceedings)
        if (_cachedRayHits == null || _cachedRayHitsFrame != Time.frameCount) {
            _cachedRayHits = visionComponent.PerformRaycasts();
            _cachedRayHitsFrame = Time.frameCount;
        }

        // Знаходимо найкращий об'єкт та дистанцію
        var bestResult = FindBestVisibleColorObject(_cachedRayHits);
        if (bestResult.found && bestResult.similarity > 0.9f) { // поріг можна налаштувати
            if (bestResult.normalizedDistance < _lastBestDistance) {
                AddReward(approachReward);
            }
            _lastBestDistance = bestResult.normalizedDistance;
        } else {
            _lastBestDistance = 1f;
        }
    }

    /// <summary>
    /// Повертає кортеж з інформацією про найкращий видимий ColorObject у списку rayHits.
    /// Це допомагає уникнути повторення коду.
    /// </summary>
    private (bool found, ColorObject colorObject, float similarity, float normalizedDistance) FindBestVisibleColorObject(IReadOnlyList<VisionComponent.RayHitInfo> rayHits) {
        if (rayHits == null) return (false, null, 0f, 1f);

        ColorObject bestMatch = null;
        float bestSimilarity = 0f;
        float bestDistance = 1f;

        foreach (var item in rayHits) {
            if (!item.hasHit || item.hitObject == null) continue;

            if (!item.hitObject.CompareTag("Color Object")) continue;

            if (!item.hitObject.TryGetComponent(out ColorObject colorObject) || colorObject == null) continue;

            float similarity = CalculateColorSimilarity(colorObject.ColorInfo.color, targetInfo.color);

            if (similarity > bestSimilarity) {
                bestSimilarity = similarity;
                bestMatch = colorObject;
                bestDistance = item.normalizedDistance;
            }
        }

        if (bestMatch != null) return (true, bestMatch, bestSimilarity, bestDistance);
        return (false, null, 0f, 1f);
    }

    private void OnCollisionEnter(Collision collision) {
        // Якщо ми в процесі ресету — ігноруємо колізії
        if (isResetting) return;

        if (collision.gameObject.CompareTag("Color Object") &&
            collision.gameObject.TryGetComponent(out ColorObject colorObject) && colorObject != null) {

            // Зберігаємо необхідні дані локально, щоб не звертатись до компонента після EndEpisode()
            ColorInfo info = colorObject.ColorInfo;
            if (info != null) {
                ProceedDecision(info.color);
            }
        }
    }

    /// <summary>
    /// Процедура прийняття рішення — приймає колір (struct Color) а не посилання на об'єкт.
    /// Вона негайно перевіряє isResetting і працює тільки з локальними даними,
    /// щоб уникнути доступу до вже знищених об'єктів після EndEpisode().
    /// </summary>
    private void ProceedDecision(Color choosenColor) {
        if (isResetting) return;

        // Копіюємо всі необхідні дані локально (щоб не посилатись на ігрові обʼєкти після EndEpisode)
        bool success = false;
        try {
            success = colorGame.TryChooseColor(choosenColor);
        } catch (Exception ex) {
            Debug.LogWarning($"Exception during TryChooseColor: {ex.Message}");
            success = false;
        }

        if (success) {
            AddReward(correctReward);
            Debug.Log($"<color=green>Correct color! Reward: {correctReward}</color>");

            float timeBonus = Mathf.Clamp01(1f - (StepCount / 1000f)) * 0.5f;
            AddReward(timeBonus);
        } else {
            AddReward(wrongPenalty);
            Debug.Log($"<color=red>Wrong color! Penalty: {wrongPenalty}</color>");
        }

        // Завершуємо епізод — після цього сцена/менеджер може знищувати об'єкти
        EndEpisode();
    }

    private void OnTriggerEnter(Collider other) {
        if (isResetting) return;

        if (other.CompareTag("Death")) {
            Debug.Log($"<color=red>Death zone! Penalty: {deathPenalty}</color>");
            AddReward(deathPenalty);
            EndEpisode();
        }
    }

    private void OnDrawGizmos() {
        if (!Application.isPlaying || visionComponent == null) return;

        if (targetInfo != null) {
            Gizmos.color = targetInfo.color;
            Gizmos.DrawWireCube(transform.position + Vector3.up * 2f, Vector3.one * 0.3f);
        }
    }
}
