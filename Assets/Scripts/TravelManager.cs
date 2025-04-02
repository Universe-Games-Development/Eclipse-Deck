using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class TravelManager {
    public Action<Room> OnRoomChanged;

    private DungeonGraph _currentDungeon;
    public Room CurrentRoom;
    private LocationData _currentLocationData;

    [Inject] private PlayerPresenter _playerPresenter;
    [Inject] private LocationTransitionManager _locationManager;
    [Inject] private RoomPresenter _roomPresenter;
    [Inject] private IDungeonGenerator _dungeonGenerator;
    [Inject] private VisitedLocationsService visitedLocationService;

    [Inject] GameEventBus _eventBus;
    [Inject]
    public void Construct(LocationTransitionManager locationManager) {
        _locationManager = locationManager;
    }

    public void BeginRun() {
        _locationManager.RegisterListener(HandleLocationTransition);
    }

    private void HandleLocationTransition(LocationData data, LoadingPhase phase = LoadingPhase.Complete) {
        switch (phase) {
            case LoadingPhase.PreLoad:
                _dungeonGenerator.ClearDungeon();
                break;
            case LoadingPhase.Complete:
                EnterLocation(data).Forget();
                break;
        }
    }

    private async UniTask EnterLocation(LocationData locationData) {
        // Genereting next location
        if (!_dungeonGenerator.GenerateDungeon(locationData, out _currentDungeon)) {
            Debug.LogError("Failed to generate dungeon");
            return;
        }
        visitedLocationService.AddVisitedLocation(locationData.locationType);
        // Entering start room
        var entranceRoom = _currentDungeon.GetEntranceNode().Room;

        _eventBus.Raise(new LocationChangedEvent(locationData, _currentLocationData));
        _currentLocationData = locationData;
        await GoToRoom(entranceRoom);
    }

    public async UniTask GoToRoom(Room chosenRoom) {
        if (chosenRoom == null)
            throw new ArgumentNullException(nameof(chosenRoom));

        // Exiting from current room
        if (CurrentRoom != null) {
        _eventBus.Raise(new RoomExitingEvent(CurrentRoom));
            await _playerPresenter.ExitRoom();
        }

        // Set new currentRoom
        CurrentRoom = chosenRoom;

        // Moving player to next room
        _roomPresenter.InitializeRoom(chosenRoom);

        _eventBus.Raise(new RoomEnteringEvent(chosenRoom));
        await _playerPresenter.EnterRoom(chosenRoom);

        OnRoomChanged?.Invoke(chosenRoom);
    }
}

public class VisitedLocationsService {
    private List<LocationType> _visitedLocations = new();

    public void AddVisitedLocation(LocationType location) {
        if (!_visitedLocations.Contains(location)) {
            _visitedLocations.Add(location);
        }
    }

    public List<LocationType> GetVisitedLocations() {
        return new List<LocationType>(_visitedLocations);
    }

    public void ClearVisitedLocations() {
        _visitedLocations.Clear();
    }
}

public struct LocationChangedEvent : IEvent {
    private LocationData locationData;
    private LocationData currentLocationData;

    public LocationChangedEvent(LocationData locationData, LocationData currentLocationData) : this() {
        this.locationData = locationData;
        this.currentLocationData = currentLocationData;
    }
}

public struct RoomExitingEvent : IEvent {
    public Room exitedRoom;

    public RoomExitingEvent(Room chosenRoom) {
        exitedRoom = chosenRoom;
    }
}

public struct RoomEnteringEvent : IEvent {
    private Room chosenRoom;

    public RoomEnteringEvent(Room chosenRoom) {
        this.chosenRoom = chosenRoom;
    }
}
