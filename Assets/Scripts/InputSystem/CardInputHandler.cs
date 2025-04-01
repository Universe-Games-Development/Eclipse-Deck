using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

public class CardInputHandler : MonoBehaviour, InputSystem_Actions.ICardBattleInputsActions {
    public InputActionMap _cardBattleMap;

    public Vector2 moveInput;       // WASD/стрілки
    public Vector2 mousePosition;   // Позиція курсора миші на екрані
    public Vector3 worldPosition;   // Позиція курсора у світовому просторі
    public bool isDragging;         // Стан перетягування
    public bool isLeftClicking;     // Стан лівої кнопки миші
    public bool isRightClicking;    // Стан правої кнопки миші
    public GameObject hoveredObject; // Об'єкт під курсором

    public event System.Action OnLeftClickPerformed;  // Подія натискання лівої кнопки
    public event System.Action OnLeftClickCanceled;   // Подія відпускання лівої кнопки
    public event System.Action OnRightClickPerformed; // Подія натискання правої кнопки
    public event System.Action OnRightClickCanceled;  // Подія відпускання правої кнопки
    public event System.Action<Vector2> OnDragPerformed;      // Подія при перетягуванні
    public event System.Action OnDragStarted;         // Подія початку перетягування
    public event System.Action OnDragEnded;           // Подія завершення перетягування

    [Header("Input Settings")]
    [SerializeField] private float dragThreshold = 5f; // Поріг для визначення перетягування
    [SerializeField] private LayerMask raycastLayers = -1; // Шари для визначення об'єктів під курсором

    private Vector2 dragStartPosition;
    private Camera mainCamera;
    private bool potentialDrag = false;

    private InputMapManager _inputMapManager;

    [Inject]
    public void Construct(InputMapManager inputManager) {
        _inputMapManager = inputManager;
    }

    private void Awake() {
        _cardBattleMap = _inputMapManager.inputAsset.CardBattleInputs;
        mainCamera = Camera.main;
    }

    private void OnEnable() {
        _inputMapManager.ToggleActionMap(_cardBattleMap);
        _inputMapManager.OnMapChanged += CheckCurrentMap;
        CheckCurrentMap(_inputMapManager.enabledMap);
    }

    private void OnDisable() {
        _inputMapManager.OnMapChanged -= CheckCurrentMap;
        _inputMapManager.inputAsset.CardBattleInputs.RemoveCallbacks(this);
        ResetValues();
    }

    private void CheckCurrentMap(InputActionMap map) {
        if (map == _cardBattleMap) {
            _inputMapManager.inputAsset.CardBattleInputs.SetCallbacks(this);
        } else {
            _inputMapManager.inputAsset.CardBattleInputs.RemoveCallbacks(this);
            ResetValues();
        }
    }
    private void ResetValues() {
        moveInput = Vector2.zero;
        mousePosition = Vector2.zero;
        worldPosition = Vector3.zero;
        isDragging = false;
        isLeftClicking = false;
        isRightClicking = false;
        hoveredObject = null;
    }

    private void Update() {
        // Оновлення світової позиції курсора
        UpdateWorldPosition();

        // Перевірка об'єкта під курсором
        UpdateHoveredObject();

        // Обробка перетягування
        ProcessDragging();
    }

    private void UpdateWorldPosition() {
        // Перетворення позиції миші з екранних координат у світові
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, raycastLayers)) {
            worldPosition = hit.point;
        }
    }

    private void UpdateHoveredObject() {
        // Визначення об'єкта під курсором
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, raycastLayers)) {
            hoveredObject = hit.collider.gameObject;
        } else {
            hoveredObject = null;
        }
    }

    private void ProcessDragging() {
        // Обробка стану перетягування
        if (potentialDrag && isLeftClicking) {
            if (!isDragging && Vector2.Distance(dragStartPosition, mousePosition) > dragThreshold) {
                isDragging = true;
                OnDragStarted?.Invoke();
            }

            if (isDragging) {
                OnDragPerformed?.Invoke(mousePosition - dragStartPosition);
            }
        } else if (isDragging && !isLeftClicking) {
            isDragging = false;
            potentialDrag = false;
            OnDragEnded?.Invoke();
        }
    }

    public void OnMove(InputAction.CallbackContext context) {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnLeftClick(InputAction.CallbackContext context) {
        isLeftClicking = context.ReadValueAsButton(); 
        //Debug.Log($"Phase: {context.phase} | Started : {context.started} | Performed: {context.performed} | Canceled: {context.canceled}");
        if (context.performed) {
            OnLeftClickPerformed?.Invoke();
            potentialDrag = true;
            dragStartPosition = mousePosition;
        } else if (context.canceled) {
            OnLeftClickCanceled?.Invoke();
        }
    }

    public void OnRightClick(InputAction.CallbackContext context) {
        isRightClicking = context.ReadValueAsButton();

        if (context.performed) {
            OnRightClickPerformed?.Invoke();
        } else if (context.canceled) {
            OnRightClickCanceled?.Invoke();
        }
    }

    public Vector3 GetWorldMousePosition() {
        return worldPosition;
    }

    public bool IsHovering(GameObject gameObject) {
        return hoveredObject == gameObject;
    }

    public bool IsHoveringAny() {
        return hoveredObject != null;
    }

    public Vector2 GetDragDelta() {
        if (isDragging) {
            return mousePosition - dragStartPosition;
        }
        return Vector2.zero;
    }

    public void OnCursorPosition(InputAction.CallbackContext context) {
        mousePosition = context.ReadValue<Vector2>();
    }
}