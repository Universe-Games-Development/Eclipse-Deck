using UnityEngine;
using Zenject;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;

public class DungeonMapUIController : MonoBehaviour {
    [Header("References")]
    [SerializeField] private Button _nextRoomsButton; // Головна кнопка "Далі"
    [SerializeField] private GameObject _roomsMenu;   // Панель з вибором кімнат

    [SerializeField] private RoomButton _roomButtonPrefab;
    [SerializeField] private Transform _buttonsContainer; // Контейнер для кнопок кімнат

    [Inject] private TravelManager _travelManager;

    private void Awake() {
        _travelManager.OnRoomChanged += HandleRoomChanged;
        _nextRoomsButton.onClick.AddListener(ToggleRoomsMenu);
        UpdateNavigationButtonState();
    }

    private void HandleRoomChanged(Room currentRoom) {
        UpdateNavigationButtonState(currentRoom);
    }

    public void UpdateNavigationButtonState(Room currentRoom = null) {
        // If there is no room
        if (currentRoom == null) {
            _nextRoomsButton.interactable = false;
            return;
        }
        if (currentRoom.isCleared) {
            OnRoomCleared(currentRoom);
        } else {
            currentRoom.OnCleared += OnRoomCleared;
        }
    }

    private void OnRoomCleared(Room currentRoom) {
        var hasNextRooms = currentRoom.Node.nextLevelConnections
            .Any(n => n.room != null);
        _nextRoomsButton.interactable = hasNextRooms;
        currentRoom.OnCleared -= OnRoomCleared;
    }

    // Готуємо кнопки для наступних кімнат
    public void PrepareRoomButtons(Room currentRoom) {
        ClearRoomButtons();

        var nextRooms = currentRoom.Node.nextLevelConnections
            .Select(n => n.room)
            .Where(r => r != null)
            .ToList();

        foreach (var room in nextRooms) {
            var button = Instantiate(_roomButtonPrefab, _buttonsContainer);
            
            button.Initialize(
                room.Data,
                () => OnRoomSelected(room)
            );
        }
    }

    private void OnRoomSelected(Room selectedRoom) {
        _travelManager.GoToRoom(selectedRoom).Forget();
        CloseMenu();
    }

    private void ToggleRoomsMenu() {
        _roomsMenu.SetActive(!_roomsMenu.activeSelf);

        if (_roomsMenu.activeSelf) {
            PrepareRoomButtons(_travelManager.CurrentRoom);
        }
    }

    public void CloseMenu() {
        _roomsMenu.SetActive(false);
    }

    private void ClearRoomButtons() {
        foreach (Transform child in _buttonsContainer) {
            Destroy(child.gameObject);
        }
    }

    private void OnDestroy() {
        if (_travelManager != null) {
            _travelManager.OnRoomChanged -= HandleRoomChanged;
        }

        _nextRoomsButton.onClick.RemoveAllListeners();
    }

    public void ToggleNextLevelButton(bool value) {
        _nextRoomsButton.gameObject.SetActive(value);
    }
}