using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class ColorMatchAgent : Agent {
    [SerializeField] ColorObject myColorBody;
    [Header("Ссылки на объекты")]
    public TextMeshProUGUI targetColorText;
    [SerializeField] ColorGame colorGame;

    [Header("Настройки обучения")]
    public bool randomizeColorsEachEpisode = true;
    public float correctReward = 1.0f;
    public float incorrectPenalty = -1.0f;

    [Header("Movement Config")]
    [SerializeField] private float hoverHeight = 1.5f;

    private int episodeCount = 0;
    private int correctChoices = 0;

    [SerializeField] Color normalColor = Color.white;
    [SerializeField] Color badDecision = Color.red;
    [SerializeField] Color rightDecision = Color.green;

    private Coroutine currentMovementCoroutine;

    private ColorInfo target;

    public override void Initialize() {
        if (colorGame == null) {
            Debug.LogError("ColorObjectsManager не назначен!");
            return;
        }
        var behaviorParams = GetComponent<BehaviorParameters>();
        if (behaviorParams != null) {
            Debug.Log($"Action Space: {behaviorParams.BrainParameters.ActionSpec}");
            Debug.Log($"Action Size: {string.Join(", ", behaviorParams.BrainParameters.ActionSpec.BranchSizes)}");
        }
    }

    public override void OnEpisodeBegin() {
        // Останавливаем любую активную корутину
        if (currentMovementCoroutine != null) {
            StopCoroutine(currentMovementCoroutine);
            currentMovementCoroutine = null;
        }
        isChoosen = false;
        SetColor(normalColor);

        colorGame.ResetGame();
        colorGame.StartGame();
        target = colorGame.GetTargetColor();

        if (targetColorText != null) {
            targetColorText.text = $"Find color: {target.colorName.ToUpper()}";
            targetColorText.color = target.color;
            targetColorText.alpha = 1f;
        }

        Debug.Log($"New Episode {episodeCount + 1}: Target Color is {target.colorName}");

        episodeCount++;
        StartCoroutine(RequestDecisionAfterDelay());
    }

    [SerializeField] float decisionDelay = 0.3f;
    private IEnumerator RequestDecisionAfterDelay() {
        yield return new WaitForSeconds(decisionDelay);
        RequestDecision();
    }

    public override void CollectObservations(VectorSensor sensor) {
        List<ColorInfo> currentColors = colorGame.GetAllColors();
        int maxColors = 9; // Максимальна кількість кольорів, яку очікуємо

        // ✅ Додаємо загальну кількість кольорів
        sensor.AddObservation((float)currentColors.Count / maxColors);

        for (int i = 0; i < maxColors; i++) {
            if (i < currentColors.Count) {
                ColorInfo colorInfo = currentColors[i];

                // RGB кольору
                sensor.AddObservation(colorInfo.color.r);
                sensor.AddObservation(colorInfo.color.g);
                sensor.AddObservation(colorInfo.color.b);

                // Схожість з цільовим кольором
                float similarity = CalculateColorSimilarity(colorInfo.color, target.color);
                sensor.AddObservation(similarity);

                // ✅ Нормалізований індекс
                sensor.AddObservation((float)i / maxColors);
            } else {
                // ✅ Заповнюємо нулями для відсутніх кольорів
                sensor.AddObservation(0f); // R
                sensor.AddObservation(0f); // G
                sensor.AddObservation(0f); // B
                sensor.AddObservation(0f); // similarity
                sensor.AddObservation(-1f); // індекс (позначає відсутність)
            }
        }

        // Цільовий колір
        sensor.AddObservation(target.color.r);
        sensor.AddObservation(target.color.g);
        sensor.AddObservation(target.color.b);
    }

    /// <summary>
    /// Вычисляет сходство между двумя цветами (0-1)
    /// </summary>
    private float CalculateColorSimilarity(Color c1, Color c2) {
        float rDiff = c1.r - c2.r;
        float gDiff = c1.g - c2.g;
        float bDiff = c1.b - c2.b;
        float distance = Mathf.Sqrt(rDiff * rDiff + gDiff * gDiff + bDiff * bDiff);

        // Преобразуем расстояние в сходство (1 = идентичные, 0 = совершенно разные)
        return Mathf.Clamp01(1f - distance / Mathf.Sqrt(3f)); // √3 - максимальное расстояние в RGB
    }

    public override void OnActionReceived(ActionBuffers actions) {
        if (isChoosen) return;

        isChoosen = true;
        int chosenObjectIndex = actions.DiscreteActions[0];
        List<ColorInfo> currentColors = colorGame.GetAllColors();

        Debug.Log($"Agent chosen index: {chosenObjectIndex}");

        if (chosenObjectIndex < 0 || chosenObjectIndex >= currentColors.Count) {
            Debug.LogWarning($"Invalid object index: {chosenObjectIndex}. Available: {currentColors.Count}");
            AddReward(incorrectPenalty * 0.5f);
            EndEpisode();
            return;
        }

        ColorObject chosenObject = colorGame.GetColorObject(currentColors[chosenObjectIndex]);
        if (chosenObject == null) {
            Debug.LogWarning("Chosen object is null!");
            AddReward(incorrectPenalty * 0.5f);
            EndEpisode();
            return;
        }

        ColorInfo colorInfo = chosenObject.ColorInfo;
        if (colorInfo == null) {
            Debug.LogWarning("Can't compare null color info");
            AddReward(incorrectPenalty * 0.5f);
            EndEpisode();
            return;
        }

        bool isCorrect = colorGame.TryChooseColor(colorInfo.color);

        if (isCorrect) {
            SetColor(rightDecision);
            AddReward(correctReward);
            correctChoices++;
            Debug.Log($"<color=green>[{episodeCount}]✓ index {chosenObjectIndex} Правильно! {colorInfo.colorName}</color> - точність {(correctChoices * 100f / episodeCount):F1}%");
        } else {
            SetColor(badDecision);
            AddReward(incorrectPenalty);

            int correctIndex = -1;
            for (int i = 0; i < currentColors.Count; i++) {
                if (currentColors[i].color == target.color) {
                    correctIndex = i;
                }
            }
            Debug.Log($"<color=red>[{episodeCount}]✗ Неправильно!</color> index {chosenObjectIndex} Обрано {colorInfo.colorName}, потрібен index {correctIndex} {target.colorName}");
        }

        if (doAnimation) {
            currentMovementCoroutine = StartCoroutine(MoveToChoosenObject(chosenObject, isCorrect));
        } else {
            EndEpisode();
        }
    }

    [SerializeField] bool isChoosen = false;
    [SerializeField] bool doAnimation = false;
    [SerializeField] float animationTime = 0.5f;
    [SerializeField] private float highlightDuration = 1f;
    [SerializeField, Range(0f, 1f)] private float moveCompletionRatio = 0.8f;

    private IEnumerator MoveToChoosenObject(ColorObject choosenObject, bool isCorrect) {
        // Choosen highlight
        Color highlightColor = isCorrect ? rightDecision : badDecision;
        choosenObject.HighlightOverTime(highlightColor, highlightDuration);

        if (!isCorrect) {
            ColorObject colorObject = colorGame.GetRightObject();
            colorObject.HighlightOverTime(rightDecision, highlightDuration);
        }

        Vector3 startPosition = transform.position;
        Vector3 endPosition = choosenObject.transform.position + Vector3.up * hoverHeight;

        float moveDuration = animationTime * moveCompletionRatio;
        float elapsed = 0f;

        while (elapsed < moveDuration) {
            float t = elapsed / moveDuration;
            t = Mathf.SmoothStep(0f, 1f, t);
            transform.position = Vector3.Lerp(startPosition, endPosition, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = endPosition;
        float remainingTime = animationTime - moveDuration;
        if (remainingTime > 0f)
            yield return new WaitForSeconds(remainingTime);

        EndEpisode();
    }

    public override void Heuristic(in ActionBuffers actionsOut) {
        var discreteActions = actionsOut.DiscreteActions;
        List<ColorInfo> currentColors = colorGame.GetAllColors();

        for (int i = 0; i < currentColors.Count && i < 9; i++) {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i)) {
                discreteActions[0] = i;
                ColorObject obj = colorGame.GetColorObject(currentColors[i]);
                Debug.Log($"Heuristic selected: {i} ({obj})");
                return;
            }
        }
    }


    private void SetColor(Color newColor) {
        if (myColorBody != null)
            myColorBody.ChangeBodyColor(newColor);
    }

    // Для отладки - логируем наблюдения
    private void LogObservations() {
        List<ColorInfo> currentColors = colorGame.GetAllColors();
        string observations = $"Target: {target.colorName} (R:{target.color.r:F2}, G:{target.color.g:F2}, B:{target.color.b:F2})\n";

        for (int i = 0; i < currentColors.Count; i++) {
            float similarity = CalculateColorSimilarity(currentColors[i].color, target.color);
            observations += $"Color {i}: {currentColors[i].colorName} (R:{currentColors[i].color.r:F2}, G:{currentColors[i].color.g:F2}, B:{currentColors[i].color.b:F2}) Similarity: {similarity:F2}\n";
        }

        Debug.Log(observations);
    }
}
