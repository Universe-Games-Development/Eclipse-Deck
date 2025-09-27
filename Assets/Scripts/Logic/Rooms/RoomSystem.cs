using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.Splines;

public class RoomSystem : MonoBehaviour {
    [SerializeField] private RoomView roomView;
    [SerializeField] private SplineContainer playerEntrySpline;
    [SerializeField] private SplineContainer playerExitSpline;
    [SerializeField] private SplineContainer enemyEntrySpline;
    [SerializeField] private bool isEnabled;
    public Room CurrentRoom { get; private set; }

    private void Awake() {
        if (roomView == null)
            Debug.LogError("RoomView reference missing in RoomPresenter");
    }

    public void InitializeRoom(Room room) {
        if (room == null) {
            Debug.LogError("Attempted to initialize RoomPresenter with null room");
            return;
        }
        if (!isEnabled) return;
        CurrentRoom = room;

        if (roomView != null && CurrentRoom.Data != null)
            roomView.InitializeView(CurrentRoom.Data);
    }
    public SplineContainer GetEntrySplineForOpponent(Opponent opponent, Room chosenRoom) {
        return roomView?.playerEntrySpline;
        throw new ArgumentException("Unknown opponent type");
    }

    public SplineContainer GetExitSplineForOpponent(Opponent opponent, Room exitedRoom) {
        return roomView?.playerExitSpline;
        throw new ArgumentException("Unknown opponent type");
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

    public void SetActivity(RoomActivity activity) {
        if (_disposed) return;

        // Clean up previous activity
        CleanupCurrentActivity();

        _currentActivity = activity;
    }

    public void Enter() {
        if (_disposed) return;
        BeginActivity();
        OnEntered?.Invoke();
    }

    public void BeginActivity() {
        if (_currentActivity == null) {
            SetCleared();
            return;
        }

        if (!_currentActivity.BlocksRoomClear) {
            SetCleared();
        } else {
            _currentActivity.OnActivityCompleted += SetCleared;
        }

        _currentActivity.Initialize(this);
    }

    private void CleanupCurrentActivity() {
        if (_currentActivity != null) {
            _currentActivity.OnActivityCompleted -= SetCleared;
            _currentActivity.Dispose();
            _currentActivity = null;
        }
    }

    public void SetCleared() {
        if (_disposed || IsCleared == true) return;

        IsCleared = true;
        OnCleared?.Invoke(this);
    }

    public string GetName() {
        if (_currentActivity != null && !string.IsNullOrEmpty(_currentActivity.Name)) {
            return _currentActivity.Name;
        } else if (!string.IsNullOrEmpty(Data.Name)) {
            return Data.Name;
        } else {
            return "Boring room";
        }
    }

    public void Exit() {
        CleanupCurrentActivity();
    }

    public void Dispose() {
        if (_disposed) return;
        _disposed = true;

        CleanupCurrentActivity();

        Node.ClearRoom();
        Node = null;
        // Очищаємо делегати
        OnCleared = null;
        OnEntered = null;
    }
}
