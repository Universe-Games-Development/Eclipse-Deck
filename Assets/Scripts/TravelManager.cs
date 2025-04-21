using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class TravelManager : MonoBehaviour {
    public Action<Room> OnRoomChanged;

    public DungeonGraph CurrentDungeon { get; private set; }
    public Room CurrentRoom;
    private LocationData _currentLocationData;

    [Inject] private RoomSystem _roomPresenter;
    [Inject] private IDungeonGenerator _dungeonGenerator;
    [Inject] private VisitedLocationsService visitedLocationService;
    [Inject] GameEventBus _eventBus;
    [Inject] BattleRegistrator _opponentRegistrator;
    [Inject] PlayerManager playerManager;
    [SerializeField] private PlayerView playerView;

    [Inject] private LocationTransitionManager _locationManager;
    private Player CurrentPlayer;

    private void Start() {
        playerManager.GetPlayer(out CurrentPlayer);
        new PlayerPresenter(CurrentPlayer, playerView);

        _opponentRegistrator.RegisterPlayer(CurrentPlayer);
        HandlePlayerAppearance(CurrentPlayer);
    }

    private void HandlePlayerAppearance(Player player) {
        CurrentPlayer = player;
        _locationManager.RegisterListener(LoadingPhase.PreLoad, ClearDungeon);
        _locationManager.RegisterListener(LoadingPhase.Complete, EnterLocationAsync);
    }

    private async UniTask ClearDungeon(LocationData data) {
        if (CurrentDungeon != null) {
            CurrentDungeon.Clear();
        }
        await UniTask.CompletedTask;
    }

    private async UniTask EnterLocationAsync(LocationData locationData) {
        // Genereting next location
        if (!_dungeonGenerator.GenerateDungeon(locationData, out DungeonGraph dungeon)) {
            Debug.LogError("Failed to generate dungeon");
            return;
        }
        CurrentDungeon = dungeon;
        visitedLocationService.AddVisitedLocation(locationData);
        // Entering start room
        var entranceRoom = CurrentDungeon.GetEntranceNode().Room;

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
            await CurrentPlayer.ExitRoom();
        }

        // Set new currentRoom
        CurrentRoom = chosenRoom;

        // Moving player to next room
        _roomPresenter.InitializeRoom(chosenRoom);

        _eventBus.Raise(new RoomEnteringEvent(chosenRoom));
        await CurrentPlayer.EnterRoom(chosenRoom);

        OnRoomChanged?.Invoke(chosenRoom);
    }
}

public class VisitedLocationsService {
    private List<LocationData> _visitedLocations = new();

    public void AddVisitedLocation(LocationData location) {
        if (!_visitedLocations.Contains(location)) {
            _visitedLocations.Add(location);
        }
    }

    public List<LocationData> GetVisitedLocations() {
        return new List<LocationData>(_visitedLocations);
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
