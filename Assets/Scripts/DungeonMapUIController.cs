using Cysharp.Threading.Tasks;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public interface IDungeonUIService {
    void ToggleNextLevelButton(bool value);
    void UpdateNavigationButtonState(Room currentRoom = null);
    void CloseMenu();
}

public class DungeonMapUIController : MonoBehaviour, IDungeonUIService {
    [Header("References")]
    [SerializeField] private Button _nextRoomsButton; // Головна кнопка "Далі"
    [SerializeField] private GameObject _roomsMenu;   // Панель з вибором кімнат
    [SerializeField] private RoomButton _roomButtonPrefab;
    [SerializeField] private Transform _buttonsContainer; // Контейнер для кнопок кімнат

    [Inject] private TravelManager _travelManager;

    private void Awake() {
        if (_travelManager == null) {
            Debug.LogError("TravelManager not injected into DungeonMapUIController");
            return;
        }

        _travelManager.OnRoomChanged += HandleRoomChanged;

        if (_nextRoomsButton != null)
            _nextRoomsButton.onClick.AddListener(ToggleRoomsMenu);
        else
            Debug.LogError("NextRoomsButton reference missing in DungeonMapUIController");

        UpdateNavigationButtonState();
    }

    private void HandleRoomChanged(Room currentRoom) {
        if (currentRoom == null) {
            Debug.LogWarning("Received null room in HandleRoomChanged");
            return;
        }

        UpdateNavigationButtonState(currentRoom);
    }

    public void UpdateNavigationButtonState(Room currentRoom = null) {
        // Перевірка наявності кнопки
        if (_nextRoomsButton == null)
            return;

        // If there is no room
        if (currentRoom == null) {
            _nextRoomsButton.interactable = false;
            return;
        }

        if (currentRoom.IsCleared) // Публічна властивість замість поля
        {
            OnRoomCleared(currentRoom);
        } else {
            // Відписуємось від попередніх подій для запобігання дублюванню
            currentRoom.OnCleared -= OnRoomCleared;
            currentRoom.OnCleared += OnRoomCleared;
        }
    }

    private void OnRoomCleared(Room currentRoom) {
        if (currentRoom == null || currentRoom.Node == null)
            return;

        var hasNextRooms = currentRoom.Node.nextLevelConnections != null &&
                           currentRoom.Node.nextLevelConnections
                               .Any(n => n != null && n.room != null);

        if (_nextRoomsButton != null)
            _nextRoomsButton.interactable = hasNextRooms;

        currentRoom.OnCleared -= OnRoomCleared;
    }

    // Готуємо кнопки для наступних кімнат
    public void PrepareRoomButtons(Room currentRoom) {
        if (currentRoom == null || currentRoom.Node == null)
            return;

        ClearRoomButtons();

        var nextRooms = currentRoom.Node.nextLevelConnections?
            .Where(n => n != null)
            .Select(n => n.room)
            .Where(r => r != null)
            .ToList();

        if (nextRooms == null || nextRooms.Count == 0)
            return;

        foreach (var room in nextRooms) {
            if (_roomButtonPrefab == null || _buttonsContainer == null)
                continue;

            var button = Instantiate(_roomButtonPrefab, _buttonsContainer);

            if (room.Data != null) {
                button.Initialize(
                    room.Data,
                    () => OnRoomSelected(room)
                );
            }
        }
    }

    private void OnRoomSelected(Room selectedRoom) {
        if (selectedRoom == null || _travelManager == null)
            return;

        _travelManager.GoToRoom(selectedRoom).Forget();
        CloseMenu();
    }

    private void ToggleRoomsMenu() {
        if (_roomsMenu == null)
            return;

        _roomsMenu.SetActive(!_roomsMenu.activeSelf);

        if (_roomsMenu.activeSelf && _travelManager != null && _travelManager.CurrentRoom != null) {
            PrepareRoomButtons(_travelManager.CurrentRoom);
        }
    }

    public void CloseMenu() {
        if (_roomsMenu != null)
            _roomsMenu.SetActive(false);
    }

    private void ClearRoomButtons() {
        if (_buttonsContainer == null)
            return;

        foreach (Transform child in _buttonsContainer) {
            if (child != null)
                Destroy(child.gameObject);
        }
    }

    private void OnDestroy() {
        if (_travelManager != null) {
            _travelManager.OnRoomChanged -= HandleRoomChanged;
        }

        if (_nextRoomsButton != null)
            _nextRoomsButton.onClick.RemoveAllListeners();
    }

    public void ToggleNextLevelButton(bool value) {
        if (_nextRoomsButton != null)
            _nextRoomsButton.gameObject.SetActive(value);
    }
}