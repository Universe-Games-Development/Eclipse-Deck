using Cysharp.Threading.Tasks;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public interface IDungeonUIService {
    void UpdateNavigationButtonState(Room currentRoom = null);
    void CloseMenu();
    void UpdateLocationInfo(string locationName, int currentRoomIndex, int totalRoomCount);
}

// Класс для представления информации о местоположении
[System.Serializable]
public class LocationInfoPanel {
    [SerializeField] private GameObject _panel;
    [SerializeField] private TMPro.TextMeshProUGUI _locationNameText;
    [SerializeField] private TMPro.TextMeshProUGUI _roomProgressText;

    public void UpdateInfo(string locationName, int currentRoomLevel, int totalRoomCount) {
        if (_locationNameText != null)
            _locationNameText.text = locationName;

        if (_roomProgressText != null)
            _roomProgressText.text = $"{currentRoomLevel}/{totalRoomCount}";
    }

    public void Show(bool isVisible) {
        if (_panel != null)
            _panel.SetActive(isVisible);
    }
}

// Класс для управления меню выбора комнат
public class RoomSelectionMenu {
    private GameObject _roomsMenu;
    private RoomButton _roomButtonPrefab;
    private Transform _buttonsContainer;
    private TravelManager _travelManager;

    public RoomSelectionMenu(GameObject roomsMenu, RoomButton roomButtonPrefab, Transform buttonsContainer, TravelManager travelManager) {
        _roomsMenu = roomsMenu;
        _roomButtonPrefab = roomButtonPrefab;
        _buttonsContainer = buttonsContainer;
        _travelManager = travelManager;
    }

    public void Show(bool isVisible) {
        if (_roomsMenu == null)
            return;

        _roomsMenu.SetActive(isVisible);

        if (isVisible && _travelManager != null && _travelManager.CurrentRoom != null) {
            PrepareRoomButtons(_travelManager.CurrentRoom);
        }
    }

    public void Toggle() {
        Show(!IsVisible());
    }

    public bool IsVisible() {
        return _roomsMenu != null && _roomsMenu.activeSelf;
    }

    public void PrepareRoomButtons(Room currentRoom) {
        if (currentRoom == null || currentRoom.Node == null)
            return;

        ClearRoomButtons();

        var nextRooms = currentRoom.Node.nextLevelConnections?
            .Where(n => n != null)
            .Select(n => n.Room)
            .Where(r => r != null)
            .ToList();

        if (nextRooms == null || nextRooms.Count == 0)
            return;

        foreach (var room in nextRooms) {
            if (_roomButtonPrefab == null || _buttonsContainer == null)
                continue;

            var button = UnityEngine.Object.Instantiate(_roomButtonPrefab, _buttonsContainer);

            if (room.Data != null) {
                button.Initialize(
                    room,
                    () => OnRoomSelected(room)
                );
            }
        }
    }

    private void OnRoomSelected(Room selectedRoom) {
        if (selectedRoom == null || _travelManager == null)
            return;

        _travelManager.GoToRoom(selectedRoom).Forget();
        Show(false);
    }

    private void ClearRoomButtons() {
        if (_buttonsContainer == null)
            return;

        foreach (Transform child in _buttonsContainer) {
            if (child != null)
                UnityEngine.Object.Destroy(child.gameObject);
        }
    }
}

// Обновленный основной класс с добавленной функциональностью отображения информации о локации
public class DungeonMapUIController : MonoBehaviour, IDungeonUIService {
    [Header("Navigation")]
    [SerializeField] private Button _nextRoomsButton;
    [SerializeField] private GameObject _roomsMenu;
    [SerializeField] private RoomButton _roomButtonPrefab;
    [SerializeField] private Transform _buttonsContainer;

    [Header("Location Info")]
    [SerializeField] private LocationInfoPanel _locationInfoPanel;

    [Inject] private TravelManager _travelManager;
    [Inject] private LocationTransitionManager _locationManager;

    private RoomSelectionMenu _roomSelectionMenu;

    private void Awake() {
        ValidateReferences();
        SetupEventListeners();
        InitializeComponents();
    }

    private void ValidateReferences() {
        if (_travelManager == null)
            Debug.LogError("TravelManager not injected into DungeonMapUIController");

        if (_locationManager == null)
            Debug.LogError("LocationTransitionManager not injected into DungeonMapUIController");

        if (_nextRoomsButton == null)
            Debug.LogError("NextRoomsButton reference missing in DungeonMapUIController");
    }

    private void SetupEventListeners() {
        if (_travelManager != null) {
            _travelManager.OnRoomChanged += HandleRoomChanged;
        }

        if (_nextRoomsButton != null) {
            _nextRoomsButton.onClick.AddListener(OnNextRoomsButtonClicked);
        }
    }

    private void InitializeComponents() {
        _roomSelectionMenu = new RoomSelectionMenu(_roomsMenu, _roomButtonPrefab, _buttonsContainer, _travelManager);
        UpdateNavigationButtonState();
        UpdateLocationInfo();
    }

    private void HandleRoomChanged(Room currentRoom) {
        if (currentRoom == null) {
            Debug.LogWarning("Received null room in HandleRoomChanged");
            return;
        }

        UpdateNavigationButtonState(currentRoom);
        UpdateLocationInfo();
    }

    public void UpdateNavigationButtonState(Room currentRoom = null) {
        if (_nextRoomsButton == null)
            return;

        if (currentRoom == null) {
            ToggleNextLevelButton(false);
            _nextRoomsButton.interactable = false;
            return;
        }

        if (currentRoom.IsCleared) {
            OnRoomCleared(currentRoom);
        } else {
            currentRoom.OnCleared += OnRoomCleared;
        }
    }

    private void OnRoomCleared(Room currentRoom) {
        if (currentRoom == null || currentRoom.Node == null)
            return;

        var hasNextRooms = currentRoom.Node.nextLevelConnections != null &&
                           currentRoom.Node.nextLevelConnections
                               .Any(n => n != null && n.Room != null);

        if (_nextRoomsButton != null) {
            ToggleNextLevelButton(hasNextRooms);
            _nextRoomsButton.interactable = hasNextRooms;
        }
            

        currentRoom.OnCleared -= OnRoomCleared;
    }

    private void OnNextRoomsButtonClicked() {
        _roomSelectionMenu.Toggle();
    }

    public void CloseMenu() {
        _roomSelectionMenu.Show(false);
    }

    private void ToggleNextLevelButton(bool value) {
        if (_nextRoomsButton != null)
            _nextRoomsButton.gameObject.SetActive(value);
    }

    // Новый метод для обновления информации о локации
    public void UpdateLocationInfo(string locationName = null, int currentRoomLevel = -1, int totalRoomCount = -1) {
        if (_locationInfoPanel == null)
            return;

        // Если параметры не указаны явно, получаем их из менеджеров
        LocationData currentLocation = _locationManager.GetSceneLocation();
        if (locationName == null && _locationManager != null && currentLocation  != null) {
            locationName = currentLocation.GetName();
        }

        // Получаем текущий индекс комнаты и общее количество комнат
        if (currentRoomLevel < 0 || totalRoomCount < 0) {
            CalculateRoomProgress(out currentRoomLevel, out totalRoomCount);
        }

        _locationInfoPanel.UpdateInfo(locationName, currentRoomLevel, totalRoomCount);
        _locationInfoPanel.Show(true);
    }

    private void CalculateRoomProgress(out int currentRoomLevel, out int totalRoomCount) {
        currentRoomLevel = 0;
        totalRoomCount = 0;

        if (_travelManager == null || _locationManager == null || _travelManager.CurrentDungeon == null) {
            return;
        }

        // Получаем все комнаты текущей локации
        DungeonGraph dungeonGraph = _travelManager.CurrentDungeon;
        totalRoomCount = dungeonGraph.GetLevelCount();

        DungeonNode node = _travelManager.CurrentRoom.Node;
        // Находим индекс текущей комнаты
        if (_travelManager.CurrentRoom != null) {
            currentRoomLevel = node.level;
        }
    }

    private void OnDestroy() {
        if (_travelManager != null) {
            _travelManager.OnRoomChanged -= HandleRoomChanged;
        }

        if (_nextRoomsButton != null) {
            _nextRoomsButton.onClick.RemoveAllListeners();
        }
    }
}