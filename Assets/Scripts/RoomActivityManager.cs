using System;
using Zenject;

public class RoomActivityManager : IDisposable {
    private readonly IRoomActivityFactory _activityFactory;
    private TravelManager _travelManager;

    public RoomActivityManager(TravelManager travelManager, IRoomActivityFactory activityFactory) {
        _travelManager = travelManager;
        _travelManager.OnRoomChanged += OnRoomEntered;
        _activityFactory = activityFactory;
    }

    public void OnRoomEntered(Room room) {
        if (room == null) return;
        room.BeginActivity();
    }

    public void Dispose() {
        _travelManager.OnRoomChanged -= OnRoomEntered;
    }
}


