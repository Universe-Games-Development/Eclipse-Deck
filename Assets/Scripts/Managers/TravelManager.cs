using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class PlayerHeroFactory {
    [InjectOptional] private PlayerPresenter playerPresenter;
    [Inject] PlayerManager playerManager;
    [Inject] private DiContainer _container;

    public bool SpawnPlayer(out PlayerPresenter presenter) {
        presenter = null;
        if (!playerManager.GetPlayer(out Player player)) {
            Debug.LogError("Failed to create player");
            return false;
        }

        // Create presenter to connect model and view
        if (playerPresenter == null) {
            CharacterPresenter presenterPrefab = player.Data.presenterPrefab;
            playerPresenter = (PlayerPresenter)_container.InstantiatePrefabForComponent<CharacterPresenter>(presenterPrefab);
        }

        presenter = playerPresenter;
        playerPresenter.Initialize(player);
        return true;
    }
}

public class TravelManager : MonoBehaviour {
    public Action<Room> OnRoomChanged;
    public DungeonGraph CurrentDungeon { get; private set; }

    private LocationData _currentLocationData;

    [SerializeField] private RoomSystem _roomSystem;


    [Inject] private IDungeonGenerator _dungeonGenerator;
    [Inject] private VisitedLocationsService _visitedLocationService;
    [Inject] private IEventBus<IEvent> _eventBus;
    [Inject] private OpponentRegistrator _opponentRegistrator;
    [Inject] private PlayerHeroFactory _playerHeroFactory;
    
    

    private PlayerPresenter _playerPresenter;
    [Inject] BoardGame boardGame;
    [Inject] private LocationTransitionManager _locationManager;
    [Inject] ResourceLoadingManager loadingManager;

    private void Start() {
        if (_playerHeroFactory == null) return;
        BeginPlayerRun().Forget();
        _locationManager.RegisterListener(LoadingPhase.Complete, EnterLocationAsync);
    }

    public async UniTask BeginPlayerRun() {
        if (!_playerHeroFactory.SpawnPlayer(out _playerPresenter)) {
            Debug.LogError("Failed to spawn player");
            return;
        }

        LocationData locationData = _locationManager.GetSceneLocation();
        if (!loadingManager.IsLocationLoaded(locationData)) {
            await loadingManager.LoadResourcesForLocation(locationData);
        }

        boardGame.TookSeat(_playerPresenter);
    }

    private void ClearDungeon(LocationData data) {
        try {
            if (CurrentDungeon != null) {
                CurrentDungeon.Clear();
            }
        } catch (Exception ex) {
            Debug.LogError($"Error clearing dungeon: {ex.Message}");
        }
    }

    private async UniTask EnterLocationAsync(LocationData locationData) {
        try {
            // Generate next location
            if (!_dungeonGenerator.GenerateDungeon(locationData, out DungeonGraph dungeon)) {
                Debug.LogError("Failed to generate dungeon");
                return;
            }

            CurrentDungeon = dungeon;
            _visitedLocationService.AddVisitedLocation(locationData);

            // Enter start room
            var entranceNode = CurrentDungeon.GetEntranceNode();
            if (entranceNode == null) {
                Debug.LogError("Entrance node is null");
                return;
            }

            var entranceRoom = entranceNode.Room;
            if (_eventBus != null) {
                _eventBus.Raise(new LocationChangedEvent(locationData, _currentLocationData));
            }

            _currentLocationData = locationData;
            await GoToRoom(entranceRoom);
        } catch (Exception ex) {
            Debug.LogError($"Error entering location: {ex.Message}");
        }
    }

    public async UniTask GoToRoom(Room chosenRoom) {
        if (chosenRoom == null)
            throw new ArgumentNullException(nameof(chosenRoom));

        if (_roomSystem.CurrentRoom != null) {
            await _playerPresenter.OnRoomExited(chosenRoom);
        }

        try {
            _roomSystem.InitializeRoom(chosenRoom);
            await _playerPresenter.EnterRoom(chosenRoom);
            OnRoomChanged?.Invoke(chosenRoom);
        } catch (Exception ex) {
            Debug.LogError($"Error going to room: {ex.Message}");
            throw;
        }
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
