using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using Zenject;

public class PlayerPresenter : MonoBehaviour {
    public Player Player { get; private set; }
    [SerializeField] private CardHandUI handUI;
    [SerializeField] CameraManager cameraManager;

    private CardInputHandler _cardInputHandler;
    private GameEventBus _eventBus;
    
    [Inject] TravelManager travelManager;

    [Inject]
    public void Construct(PlayerManager playerManager, CardInputHandler cardInputHandler, GameEventBus eventBus) {
        if (!playerManager.GetPlayer(out Player player)) {
            Debug.LogWarning("Failed to get player");
            return;
        }
        Player = player;

        _cardInputHandler = cardInputHandler;
        _eventBus = eventBus;
    }

    private void Awake() {
        _cardInputHandler.OnLeftClickPerformed += HandleClick;

        cameraManager.Initialize();
        
        travelManager.BeginRun();
    }


    private void HandleClick() {
        //Debug.Log("Clicked Left Mouse Btn");
    }

    public async UniTask EnterRoom(Room chosenRoom) {
        // ����������� ��������� ��� ������� ��������
        Debug.Log($"Starting to enter room: {chosenRoom.Data.name}");

        // ��������� ������ ����� � ������
        await Player.EnterRoom(chosenRoom);
    }

    public async UniTask ExitRoom() {
        Room currentRoom = Player.GetCurrentRoom();
        if (currentRoom != null) {
            Debug.Log($"Starting to exit room: {currentRoom.Data.name}");
        }

        // �������� ������ ������ � ������
        await Player.ExitRoom();
    }

    private void OnDestroy() {
        if (_cardInputHandler != null) {
            _cardInputHandler.OnLeftClickPerformed -= HandleClick;
        }
    }
}

public class Player : Opponent {
    private Room currentRoom;
    public Func<Room, UniTask> OnRoomEntered;
    public Func<Room, UniTask> OnRoomExited;

    public Player(GameEventBus eventBus, CommandManager commandManager, CardProvider cardProvider)
        : base(eventBus, commandManager, cardProvider) {
        Name = "Player";
    }

    public async UniTask EnterRoom(Room room) {
        if (currentRoom != null) {
            currentRoom.Exit();
        }
        
        currentRoom = room;

        Debug.Log($"Room entered: {currentRoom.GetName()}");

        // Notify about room entry
        if (OnRoomEntered != null) {
            await OnRoomEntered.Invoke(room);
        }

        room.Enter();
        await UniTask.CompletedTask;
    }

    public async UniTask ExitRoom() {
        if (currentRoom != null) {
            Room exitingRoom = currentRoom;
            Debug.Log($"Exited room: {currentRoom.Data.name}");

            // Notify about room exit
            OnRoomExited?.Invoke(exitingRoom);

            currentRoom = null;
        }

        await UniTask.CompletedTask;
    }

    public Room GetCurrentRoom() {
        return currentRoom;
    }
}
