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

        RoomActivity activity = _activityFactory.CreateActivity(room);
        room.SetActivity(activity);
    }

    public void Dispose() {
        _travelManager.OnRoomChanged -= OnRoomEntered;
    }
}

public interface IRoomActivityFactory {
    RoomActivity CreateActivity(Room room);
}

public class RoomActivityFactory : IRoomActivityFactory {
    private readonly DiContainer _container;

    public RoomActivityFactory(DiContainer container) {
        _container = container;
    }

    public RoomActivity CreateActivity(Room room) {
        switch (room.Data.type) {
            case RoomType.Enemy:
            case RoomType.Boss:
                return _container.Instantiate<EnemyRoomActivity>();
            default:
                return null;
        }
    }
}
