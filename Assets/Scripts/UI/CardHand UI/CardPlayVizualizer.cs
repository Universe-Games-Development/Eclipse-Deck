using UnityEngine;

public class CardPlayVizualizer : MonoBehaviour {
    [Header("Playing State Settings")]
    [SerializeField] public BoardInputManager boardInputManager;
    [SerializeField] public LayerMask boardMask;
    [SerializeField] public Transform cursorIndicator;
    [SerializeField] public Vector3 cardOffset = new Vector3(0f, 1.2f, 0f);

    [Header("Movement Settings")]
    [SerializeField] private float followStrength = 8f;
    [SerializeField] private float drag = 5f;
    [SerializeField] private float maxVelocity = 15f;

    [Header("Tilt Physics")]
    [SerializeField] private float forwardTiltSensitivity = 2f;    // Нахил вперед/назад
    [SerializeField] private float sideTiltSensitivity = 1.5f;     // Нахил вліво/вправо
    [SerializeField] private float verticalTiltSensitivity = 0.8f; // Нахил від вертикальної швидкості
    [SerializeField] private float maxTiltAngle = 25f;
    [SerializeField] private float tiltSmoothing = 8f;
    [SerializeField] private float rotationDamping = 0.95f;        // Затухання обертання

    [Header("Physics Simulation")]
    [SerializeField] private float gravityEffect = 0.2f;          // Вплив "гравітації" на карту
    [SerializeField] private float airResistance = 2f;            // Опір повітря

    [Header("Components")]
    [SerializeField] private HumanTargetSelector selector;
    [SerializeField] private CardPlayModule cardPlayModule;

    // Runtime variables
    private CardPresenter currentCard;
    public Vector3 lastBoardPosition;
    private Vector3 cardVelocity;
    public Vector3 lastCardPosition;
    private Vector3 smoothedVelocity;
    private Vector3 angularVelocity;
    private Quaternion baseRotation;

    // Debug info
    [Header("Debug Info")]
    [SerializeField] private bool showDebugInfo = false;
    [SerializeField] private Vector3 currentVelocity;
    [SerializeField] private Vector3 currentTilt;

    private void OnEnable() {
        selector.OnSelectionStarted += HandleSelectionStarted;
    }

    private void OnDisable() {
        selector.OnSelectionStarted -= HandleSelectionStarted;
    }

    private void Start() {
        baseRotation = Quaternion.identity;
    }

    private void Update() {
        UpdateCursorPosition();

        CardMovement();
    }

    private void UpdateCursorPosition() {
        if (boardInputManager.TryGetCursorPosition(boardMask, out Vector3 cursorPosition)) {
            lastBoardPosition = cursorPosition;
            cursorIndicator.transform.position = lastBoardPosition;
        }
    }

    private void CardMovement() {
        if (currentCard == null) return;

        Vector3 currentPosition = currentCard.transform.position;

        // Замість простого додавання офсету, розраховуємо позицію на промені камери
        Vector3 targetPosition = CalculateCardPositionOnCameraRay();

        // Обчислення сили до цільової позиції
        Vector3 forceToTarget = (targetPosition - currentPosition) * followStrength;

        // Застосування "гравітації" - карта трохи падає вниз
        Vector3 gravity = Vector3.down * gravityEffect;

        // Опір повітря - зменшує швидкість
        Vector3 airDrag = -cardVelocity * airResistance;

        // Загальна сила
        Vector3 totalForce = forceToTarget + gravity + airDrag;

        // Оновлення швидкості
        cardVelocity += totalForce * Time.deltaTime;

        // Обмеження максимальної швидкості
        if (cardVelocity.magnitude > maxVelocity) {
            cardVelocity = cardVelocity.normalized * maxVelocity;
        }

        // Застосування драгу
        cardVelocity *= Mathf.Pow(1f - drag * 0.1f, Time.deltaTime);

        // Рух карти
        currentCard.transform.position = currentPosition + cardVelocity * Time.deltaTime;

        // Згладжена швидкість для більш стабільного нахилу
        smoothedVelocity = Vector3.Lerp(smoothedVelocity, cardVelocity, Time.deltaTime * 10f);

        // Обчислення нахилу
        CardTilt();

        lastCardPosition = currentCard.transform.position;

        // Debug info
        if (showDebugInfo) {
            currentVelocity = cardVelocity;
        }
    }

    [Header("Camera Ray Settings")]
    [SerializeField] private bool useCameraRayPositioning = true; // Можна вимкнути для старої поведінки

    private Vector3 CalculateCardPositionOnCameraRay() {
        // Якщо вимкнено, використовуємо стару логіку
        if (!useCameraRayPositioning) {
            return lastBoardPosition + cardOffset;
        }
        Camera mainCamera = Camera.main;
        if (mainCamera == null) {
            // Fallback до старого методу якщо камери немає
            return lastBoardPosition + cardOffset;
        }

        // Отримуємо промінь від камери до точки курсора на дошці
        Vector3 cameraPosition = mainCamera.transform.position;
        Vector3 directionToCursor = (lastBoardPosition - cameraPosition).normalized;

        // Висота на якій має бути карта над дошкою
        float cardHeightAboveBoard = cardOffset.y;

        // Висота дошки (беремо Y координату з lastBoardPosition)
        float boardHeight = lastBoardPosition.y;

        // Цільова висота для карти
        float targetCardHeight = boardHeight + cardHeightAboveBoard;

        // Розраховуємо точку на промені де Y координата дорівнює цільовій висоті
        if (Mathf.Abs(directionToCursor.y) > 0.001f) {
            // Обчислюємо відстань вздовж промені для досягнення потрібної висоти
            float distanceAlongRay = (targetCardHeight - cameraPosition.y) / directionToCursor.y;

            // Якщо відстань позитивна (промінь йде в правильному напрямку)
            if (distanceAlongRay > 0) {
                Vector3 cardPositionOnRay = cameraPosition + directionToCursor * distanceAlongRay;
                return cardPositionOnRay;
            }
        }
        Vector3 horizontalDirection = new Vector3(directionToCursor.x, 0, directionToCursor.z).normalized;
        float horizontalDistance = Vector3.Distance(
            new Vector3(cameraPosition.x, 0, cameraPosition.z),
            new Vector3(lastBoardPosition.x, 0, lastBoardPosition.z)
        );

        return new Vector3(lastBoardPosition.x, targetCardHeight, lastBoardPosition.z);
    }

    private void CardTilt() {
        if (currentCard == null) return;

        // Базові нахили на основі швидкості
        float pitchFromVelocity = -smoothedVelocity.z * forwardTiltSensitivity; // Нахил вперед/назад
        float rollFromVelocity = smoothedVelocity.x * sideTiltSensitivity;      // Нахил вліво/вправо
        float pitchFromVertical = -smoothedVelocity.y * verticalTiltSensitivity; // Від вертикальної швидкості

        // Загальний нахил
        float totalPitch = Mathf.Clamp(pitchFromVelocity + pitchFromVertical, -maxTiltAngle, maxTiltAngle);
        float totalRoll = Mathf.Clamp(rollFromVelocity, -maxTiltAngle, maxTiltAngle);

        // Додаткове обертання навколо Y-осі (yaw) на основі бічної швидкості
        float yawFromVelocity = smoothedVelocity.x * 0.5f;
        yawFromVelocity = Mathf.Clamp(yawFromVelocity, -maxTiltAngle * 0.5f, maxTiltAngle * 0.5f);

        // Цільове обертання
        Vector3 targetEuler = new Vector3(totalPitch, yawFromVelocity, totalRoll);
        Quaternion targetRotation = baseRotation * Quaternion.Euler(targetEuler);

        // Згладжене обертання
        currentCard.transform.rotation = Quaternion.Slerp(
            currentCard.transform.rotation,
            targetRotation,
            Time.deltaTime * tiltSmoothing
        );

        // Затухання кутової швидкості для більш природного руху
        angularVelocity *= rotationDamping;
        if (showDebugInfo) {
            currentTilt = targetEuler;
        }
    }

    private void HandleSelectionStarted(ITargetRequirement requirement) {
        if (currentCard == null) {
            Debug.Log("Show arrow selection from player to cursor");
            SetupArrowSelection();
            return;
        }

        if (requirement is ZoneRequirement zoneReq) {
            Debug.Log($"Show zone visualization");
        } else {
            Debug.Log("Show single target visualization");
        }
    }

    private void SetupArrowSelection() {
        // TODO: реалізація стрілки
    }

    public void SetCardPresenter(CardPresenter cardPresenter) {
        if (currentCard != null) {
            // Зберігаємо плавність при зміні карти
            cardVelocity *= 0.5f;
        }

        currentCard = cardPresenter;

        if (currentCard != null) {
            baseRotation = currentCard.transform.rotation;
            lastCardPosition = currentCard.transform.position;

            // Початкові значення для плавного переходу
            cardVelocity = Vector3.zero;
            smoothedVelocity = Vector3.zero;
            angularVelocity = Vector3.zero;
        }
    }

    public void Stop() {
        if (currentCard != null) {
            // Плавне повернення до початкового стану
            StartCoroutine(SmoothStop());
        } else {
            currentCard = null;
        }
    }

    private System.Collections.IEnumerator SmoothStop() {
        float stopDuration = 0.5f;
        float elapsed = 0f;

        Vector3 initialVelocity = cardVelocity;
        Quaternion initialRotation = currentCard.transform.rotation;

        while (elapsed < stopDuration && currentCard != null) {
            float t = elapsed / stopDuration;
            float smoothT = 1f - Mathf.Pow(1f - t, 3f); // Smooth curve

            // Зменшуємо швидкість
            cardVelocity = Vector3.Lerp(initialVelocity, Vector3.zero, smoothT);

            // Повертаємо до базового обертання
            currentCard.transform.rotation = Quaternion.Slerp(
                initialRotation,
                baseRotation,
                smoothT
            );

            elapsed += Time.deltaTime;
            yield return null;
        }

        currentCard = null;
    }

    // Додаткові методи для налаштування
    public void SetMovementParameters(float followStr, float dragVal, float maxVel) {
        followStrength = followStr;
        drag = dragVal;
        maxVelocity = maxVel;
    }

    public void SetTiltParameters(float forwardSens, float sideSens, float maxAngle) {
        forwardTiltSensitivity = forwardSens;
        sideTiltSensitivity = sideSens;
        maxTiltAngle = maxAngle;
    }
}