using UnityEngine;
using Zenject;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;
using System;

public class DungeonMapUIController : MonoBehaviour {
    [Header("References")]
    [SerializeField] private Button _nextRoomsButton; // Головна кнопка "Далі"
    [SerializeField] private GameObject _menuPanel;   // Панель з вибором кімнат

    [SerializeField] private RoomButton _roomButtonPrefab;
    [SerializeField] private Transform _buttonsContainer; // Контейнер для кнопок кімнат

    private TravelManager _travelManager;

    [Inject]
    public void Construct(TravelManager travelManager) {
        _travelManager = travelManager;
        _travelManager.OnRoomChanged += HandleRoomChanged;

        // Налаштування кнопок
        _nextRoomsButton.onClick.AddListener(ToggleMenu);
        UpdateNavigationButtonState();
    }

    private void HandleRoomChanged(Room currentRoom) {
        UpdateNavigationButtonState(currentRoom);
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

    // Оновлюємо стан кнопки навігації
    public void UpdateNavigationButtonState(Room currentRoom = null) {
        var hasNextRooms = currentRoom?.Node.nextLevelConnections
            .Any(n => n.room != null) ?? false;

        _nextRoomsButton.interactable = hasNextRooms;
    }

    private void ToggleMenu() {
        _menuPanel.SetActive(!_menuPanel.activeSelf);

        if (_menuPanel.activeSelf) {
            PrepareRoomButtons(_travelManager.CurrentRoom);
        }
    }

    public void CloseMenu() {
        _menuPanel.SetActive(false);
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