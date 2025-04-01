using System;
using UnityEngine;
using Zenject;

public class RoomPresenter : MonoBehaviour {
    [Inject] private IDungeonUIService _dungeonUI; // Використовуємо інтерфейс замість конкретного класу

    [SerializeField] private RoomView roomView;
    [SerializeField] private ActivitySpawner activitySpawner;

    public Room CurrentRoom { get; private set; }

    private void Awake() {
        if (roomView == null)
            Debug.LogError("RoomView reference missing in RoomPresenter");

        if (activitySpawner == null)
            Debug.LogError("ActivitySpawner reference missing in RoomPresenter");
    }

    public void InitializeRoom(Room room) {
        if (room == null) {
            Debug.LogError("Attempted to initialize RoomPresenter with null room");
            return;
        }

        if (CurrentRoom != null) {
            UnsubscribeRoom();
        }

        CurrentRoom = room;

        if (roomView != null && CurrentRoom.Data != null)
            roomView.InitializeView(CurrentRoom.Data);

        CurrentRoom.OnEntered += HandleEnteringRoom;
        CurrentRoom.OnCleared += HandleClearingRoom;
    }

    private void UnsubscribeRoom() {
        if (CurrentRoom != null) {
            CurrentRoom.OnEntered -= HandleEnteringRoom;
            CurrentRoom.OnCleared -= HandleClearingRoom;
        }
    }

    public void HandleEnteringRoom() {
        if (_dungeonUI != null)
            _dungeonUI.ToggleNextLevelButton(false);

        if (activitySpawner == null || CurrentRoom == null)
            return;

        RoomActivity activity = activitySpawner.GetActivity(CurrentRoom);
        CurrentRoom.SetActivity(activity);
    }

    private void HandleClearingRoom(Room currentRoom) {
        if (_dungeonUI != null)
            _dungeonUI.ToggleNextLevelButton(true);
    }

    private void OnDestroy() {
        UnsubscribeRoom();
    }
}

public class Room : IDisposable {
    public event Action<Room> OnCleared;
    public event Action OnEntered;

    public readonly RoomData Data;
    public bool IsCleared { get; private set; } // Публічна властивість замість поля

    public DungeonNode Node { get; private set; }

    private RoomActivity _currentActivity;
    private bool _disposed = false;

    public Room(DungeonNode node, RoomData roomData) {
        Data = roomData ?? throw new ArgumentNullException(nameof(roomData));
        Node = node ?? throw new ArgumentNullException(nameof(node));
        IsCleared = false;
    }

    public void Enter() {
        if (_disposed) return;
        OnEntered?.Invoke();
    }

    public void SetActivity(RoomActivity activity) {
        if (_disposed) return;

        // Clean up previous activity
        CleanupCurrentActivity();

        _currentActivity = activity;

        if (_currentActivity == null || !_currentActivity.BlocksRoomClear) {
            SetCleared();
        } else {
            _currentActivity.OnActivityCompleted += SetCleared;
            _currentActivity.Initialize();
        }
    }

    private void CleanupCurrentActivity() {
        if (_currentActivity != null) {
            _currentActivity.OnActivityCompleted -= SetCleared;
            _currentActivity.Dispose();
            _currentActivity = null;
        }
    }

    public void SetCleared(bool value = true) {
        if (_disposed || IsCleared == value) return;

        IsCleared = value;
        OnCleared?.Invoke(this);
    }

    public void Dispose() {
        if (_disposed) return;

        _disposed = true;
        CleanupCurrentActivity();

        // Очищаємо делегати
        OnCleared = null;
        OnEntered = null;
    }
}
