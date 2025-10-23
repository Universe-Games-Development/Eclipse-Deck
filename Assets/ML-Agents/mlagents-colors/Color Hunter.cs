using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

[RequireComponent(typeof(CharacterMover))]
public class ColorHunterAgent : Agent {
    [Header("Налаштування агента")]
    [SerializeField] private float maxMoveSpeed = 5f;
    [SerializeField] private Vector3 spawnPoint = Vector3.one;

    [Header("Посилання")]
    [SerializeField] private ColorObjectsManager colorManager;

    [Header("Винагороди")]
    [Tooltip("Штраф за кожен крок (заохочує швидше знаходити ціль)")]
    [SerializeField] private float stepPenalty = -0.001f;

    [Tooltip("Штраф за бездіяльність")]
    [SerializeField] private float idlePenalty = -0.002f;

    [Tooltip("Винагорода за правильний колір")]
    [SerializeField] private float correctReward = 1.0f;

    [Tooltip("Штраф за неправильний колір")]
    [SerializeField] private float wrongPenalty = -0.2f;

    [Header("Згладжування дій")]
    [Tooltip("Коефіцієнт згладжування дій (0 = без згладжування, 1 = максимальне)")]
    [Range(0f, 0.95f)]
    [SerializeField] private float actionSmoothing = 0.3f;
    private Vector3 _lastActions = Vector3.zero;

    [HideInInspector] public ColorInfo target;

    private CharacterMover _characterMover;
    private Rigidbody _agentRb;


    // Для відладки
    private Vector3 _lastMoveDirection;

    public override void Initialize() {
        _characterMover = GetComponent<CharacterMover>();
        _agentRb = GetComponent<Rigidbody>();

        if (colorManager == null) {
            colorManager = FindFirstObjectByType<ColorObjectsManager>();
        }

        colorManager.Initialize(6);
        Debug.Log("ColorHunterAgent initialized with RaycastSensorManager");
    }

    public override void OnEpisodeBegin() {
        // Скидаємо рух
        ResetState();

        // Ініціалізація мішені
        if (colorManager != null) {
            target = colorManager.CreateNewTargetColor();
        }
    }

    public override void CollectObservations(VectorSensor sensor) {
        // 1. Спостереження за станом руху (4 значення)
        if (_characterMover != null) {
            sensor.AddObservation(_characterMover.CurrentSpeed / maxMoveSpeed); // 1
            sensor.AddObservation(_characterMover.GetMoveDirection()); // 3
        } else {
            sensor.AddObservation(0f);
            sensor.AddObservation(Vector3.zero);
        }

        // 2. Напрямок погляду (3 значення)
        sensor.AddObservation(transform.forward);

        // 3. Нормалізована позиція (3 значення)
        sensor.AddObservation(transform.position / 50f);
    }

    public override void OnActionReceived(ActionBuffers actions) {
        // Отримуємо необроблені дії
        float rawForward = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        float rawRight = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);
        float rawTurn = Mathf.Clamp(actions.ContinuousActions[2], -1f, 1f);

        // Застосовуємо згладжування дій
        Vector3 currentActions = new Vector3(rawForward, rawRight, rawTurn);
        Vector3 smoothedActions = Vector3.Lerp(currentActions, _lastActions, actionSmoothing);
        _lastActions = smoothedActions;

        // Використовуємо згладжені дії
        float forwardInput = smoothedActions.x;
        float rightInput = smoothedActions.y;
        float turnInput = smoothedActions.z;

        // Застосовуємо рух
        if (_characterMover != null) {
            _characterMover.SetMovementInput(forwardInput, rightInput);
            _characterMover.SetTurnInput(turnInput);
            _lastMoveDirection = _characterMover.GetMoveDirection();
        }

        // Система винагород
        AddReward(stepPenalty);

        // Штраф за бездіяльність
        if (Mathf.Abs(forwardInput) < 0.1f && Mathf.Abs(rightInput) < 0.1f && Mathf.Abs(turnInput) < 0.1f) {
            AddReward(idlePenalty);
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut) {
        var continuousActions = actionsOut.ContinuousActions;

        // Рух вперед/назад (W/S)
        continuousActions[0] = Input.GetAxis("Vertical");

        // Рух вліво/вправо (A/D для страфу)
        continuousActions[1] = Input.GetAxis("Horizontal");

        // Поворот (Q/E)
        if (Input.GetKey(KeyCode.Q)) {
            continuousActions[2] = -1f;
        } else if (Input.GetKey(KeyCode.E)) {
            continuousActions[2] = 1f;
        } else {
            continuousActions[2] = 0f;
        }
    }


    private void OnCollisionEnter(Collision collision) {
        ColorObject colorObj = collision.gameObject.GetComponent<ColorObject>();
        if (colorObj != null) {
            SetChoosenObject(colorObj);
        }
    }

    public void SetChoosenObject(ColorObject colorObj) {
        ResetState();

        if (colorObj.GetColor() == target.color) {
            // Правильний колір
            AddReward(correctReward);
            Debug.Log($"✅ Correct! Found: {colorObj} (+{correctReward})");
        } else {
            // Неправильний колір
            AddReward(wrongPenalty);
            Debug.Log($"❌ Wrong! Found: {colorObj}, ({wrongPenalty})");
        }

        
        EndEpisode();
    }

    private void ResetState() {
        if (_characterMover != null) {
            _characterMover.StopMovement();
        }

        if (_agentRb != null) {
            _agentRb.linearVelocity = Vector3.zero;
            _agentRb.angularVelocity = Vector3.zero;
        }

        // Скидаємо згладжені дії
        _lastActions = Vector3.zero;

        // Респавн агента
        transform.position = spawnPoint;
        transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
        _lastMoveDirection = Vector3.zero;
    }
}