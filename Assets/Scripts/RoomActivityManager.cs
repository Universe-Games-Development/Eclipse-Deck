using System;
using Zenject;

public class RoomActivityManager : IDisposable {
    private TravelManager _travelManager;

    public RoomActivityManager(TravelManager travelManager) {
        _travelManager = travelManager;
        //_travelManager.OnRoomChanged += OnRoomEntered;
    }

    public void OnRoomEntered(Room room) {
        if (room == null) return;
        room.BeginActivity();
    }

    public void Dispose() {
        //_travelManager.OnRoomChanged -= OnRoomEntered;
    }
}


