using Cysharp.Threading.Tasks;
using System;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class PlayerPresenter : BaseOpponentPresenter {
    public Player Player => (Player) OpponentModel;
    
    [SerializeField] private CardHandView handUI;
    [SerializeField] private CameraManager cameraManager;

    [Inject] protected BattleRegistrator opponentRegistrator;
    [Inject] protected GameEventBus eventBus;
    [Inject] private CardInputHandler _cardInputHandler;

    private void Awake() {
        _cardInputHandler.OnLeftClickPerformed += HandleClick;
    }

    // Player-specific initialization
    public void InitializePlayer(Player player) {
        base.Initialize(player);

        // Register with the opponent system
        opponentRegistrator.RegisterPlayer(this);
    }

    private void HandleClick() {
        //Debug.Log("Clicked Left Mouse Btn");
    }

    public async UniTask EnterRoom(Room chosenRoom) {
        Debug.Log($"Starting to enter room: {chosenRoom.Data.name}");
        await cameraManager.BeginEntranse();
        Player.EnterRoom(chosenRoom);
    }

    public async UniTask ExitRoom() {
        await cameraManager.BeginExiting();
        Room currentRoom = Player.GetCurrentRoom();
        if (currentRoom != null) {
            Debug.Log($"Starting to exit room: {currentRoom.Data.name}");
            Player.ExitRoom();
        }
    }

    // Override base destroy
    protected void OnDestroy() {
        if (_cardInputHandler != null) {
            _cardInputHandler.OnLeftClickPerformed -= HandleClick;
        }
    }
}

public class Player : Opponent {
    private Room currentRoom;

    public Player(GameEventBus eventBus)
        : base(eventBus) {
    }

    public void EnterRoom(Room room) {
        if (currentRoom != null) {
            ExitRoom();
        }

        currentRoom = room;
        Debug.Log($"Room entered: {room.GetName()}");
        room.Enter();
    }

    public void ExitRoom() {
        if (currentRoom != null) {
            Room exitingRoom = currentRoom;
            currentRoom.Exit();
            Debug.Log($"Exited room: {exitingRoom.Data.name}");

            currentRoom = null;
        }
    }

    public Room GetCurrentRoom() {
        return currentRoom;
    }
}


public class BaseOpponentPresenter : MonoBehaviour {
    public Opponent OpponentModel { get; protected set; }
    // Initialize the presenter with the model
    public virtual void Initialize(Opponent opponentModel) {
        OpponentModel = opponentModel;
    }

    public async UniTask MoveTo(Transform seatTransform) {
        await MoveToPositionAsync(seatTransform);
    }
    public async UniTask MoveToPositionAsync(Transform target, float duration = 1f) {
        Vector3 start = transform.position;
        Quaternion startRot = transform.rotation;

        Vector3 end = target.position;
        Quaternion endRot = target.rotation;

        float elapsed = 0f;

        while (elapsed < duration) {
            float t = elapsed / duration;
            transform.position = Vector3.Lerp(start, end, t);
            transform.rotation = Quaternion.Slerp(startRot, endRot, t);
            elapsed += Time.deltaTime;
            await UniTask.Yield();
        }

        // Гарантуємо точну установку фінальної позиції
        transform.position = end;
        transform.rotation = endRot;
    }

    internal IActionFiller GetActionFiller() {
        throw new NotImplementedException();
    }
}