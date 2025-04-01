using System;
using UnityEngine;
using Zenject;

public class RoomPresenter : MonoBehaviour {
    [Inject] private DungeonMapUIController _mapUI;
    [SerializeField] private RoomView roomView;
    [SerializeField] private ActivitySpawner activitySpawner;
    public Room CurrentRoom { get; private set; }
    public void InitializeRoom(Room room) {
        if (CurrentRoom != null) {
            UnsubscribeRoom();
        }

        CurrentRoom = room;
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
        _mapUI.ToggleNextLevelButton(false);
        RoomActivity activity = activitySpawner.GetActivity(CurrentRoom);
        
        CurrentRoom.SetActivity(activity);
    }
    private void HandleClearingRoom(Room currentRoom) {
        _mapUI.ToggleNextLevelButton(true);
    }

    private void OnDestroy() {
        UnsubscribeRoom();
    }
}

public class Room {
    public Action<Room> OnCleared;
    public Action OnEntered;
    public readonly RoomData Data;
    public bool isCleared { get; private set; }
    
    public DungeonNode Node { get; private set; }
    
    private RoomActivity _currentActivity;

    public Room(DungeonNode node, RoomData roomData) {
        Data = roomData ?? throw new ArgumentNullException(nameof(roomData));
        Node = node;
    }

    public void Enter() {
        OnEntered?.Invoke();
    }

    public void SetActivity(RoomActivity activity) {
        // Clean up previous activity
        if (_currentActivity != null) {
            _currentActivity.OnActivityCompleted -= SetCleared;
            _currentActivity.Dispose();
        }

        _currentActivity = activity;

        if (_currentActivity == null || !_currentActivity.BlocksRoomClear) {
            SetCleared();
        } else {
            _currentActivity.OnActivityCompleted += SetCleared;
            activity.Initialize();
        }
    }

    public void SetCleared(bool value = true) {
        if (isCleared == value) return;
        isCleared = value;
        OnCleared?.Invoke(this);
    }
}
