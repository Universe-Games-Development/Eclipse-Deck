using Cysharp.Threading.Tasks;
using System;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class PlayerPresenter : MonoBehaviour {
    public Player Player;
    private TaskCompletionSource<Player> _playerInitializationSource = new TaskCompletionSource<Player>();
    [SerializeField] private CardHandUI handUI;
    [SerializeField] private RaycastService rayService;

    [Inject] DiContainer container;
    private PlayerManager playerManager;
    [Inject] OpponentRegistrator opponentRegistrator;

    [Inject]
    public void Construct(PlayerManager playerManager) {
        this.playerManager = playerManager;
        if (!playerManager.GetPlayer(out Player)) {
            Debug.LogWarning("Failed to generate player");
            return;
        }
        
    }


    private void Update() {
        if (Input.GetMouseButtonDown(0)) {
            Vector3? mouseWorldPosition = rayService.GetRayMousePosition();
            //if (gameboard_c.TryGetField(out Field field, mouseWorldPosition)) {
            //    Debug.Log("Clicked Field at: " + field.GetTextCoordinates());
            //}
        }
    }

    public async UniTask EnterRoom(Room chosenRoom) {
        await Player.EnterRoom(chosenRoom);
    }

    public async UniTask ExitRoom() {
        await Player.ExitRoom();
    }
}

public class Player : Opponent {
    private Room currentRoom;
    public Func<Room, UniTask> OnRoomEntered;

    public Player(OpponentData opponentData, GameEventBus eventBus, CommandManager commandManager, CardProvider cardProvider)
        : base(opponentData, eventBus, commandManager, cardProvider) {
        Name = "Player";
    }

    public async UniTask EnterRoom(Room room) {
        currentRoom = room;
        Debug.Log($"Room entered: {currentRoom.Data.name}");

        if (OnRoomEntered != null) {
            await OnRoomEntered.Invoke(room);
        }

        room.Enter();
        await UniTask.CompletedTask;
    }

    internal async Task ExitRoom() {
        if (currentRoom != null) {
            Debug.Log($"Exited room: {currentRoom.Data.name}");
            currentRoom = null;
        }

        await UniTask.CompletedTask;
    }
}
