using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using Zenject;

public interface IMannable {
    public Mana Mana { get; }
}

public class Opponent : IDisposable, IDamageable, IMannable {
    public Func<Room, UniTask> OnRoomEntered;
    public Func<Room, UniTask> OnRoomExited;
    public Action<Opponent> OnDefeat { get; internal set; }
    public Func<BoardSeat, UniTask> OnTookSeat;
    public Func<UniTask> OnClearedSeat;

    public Health Health { get; private set; }
    public Mana Mana { get; private set; }
    public OpponentData Data { get; private set; }

    [Inject] protected GameEventBus _eventBus;
    public Opponent(OpponentData data) {
        Data = data;
        Health = new Health(Data.Health, this, _eventBus);
        Mana = new Mana(this, Data.Mana, _eventBus);
    }
   
    public async UniTask TakeSeat(BoardSeat boardSeat) {
        if (OnTookSeat != null) {
            await OnTookSeat(boardSeat);
        }
    }

    public async UniTask ClearSeat() {
        if (OnTookSeat != null) {
            await OnClearedSeat();
        }
    }
    public override string ToString() {
        return $"{GetType().Name} {Data.Name} ({Health.CurrentValue}/{Health.TotalValue})";
    }
    public virtual void Dispose() {
        GC.SuppressFinalize(this);
    }
}

public class Player : Opponent {
    
    public PlayerData PlayerData => (PlayerData)base.Data;
    private Room currentRoom;

    public Player(PlayerData data) : base(data) {

    }

    public async UniTask EnterRoom(Room room) {
        if (currentRoom != null) {
            await ExitRoom();
        }

        if (OnRoomExited != null) {
            await OnRoomEntered.Invoke(currentRoom);
        }

        _eventBus.Raise(new RoomEnteringEvent(room));
        currentRoom = room;
        Debug.Log($"Room entered: {room.GetName()}");
        room.Enter();

    }

    public async UniTask ExitRoom() {
        if (currentRoom != null) {
            if (OnRoomExited != null) {
                await OnRoomExited.Invoke(currentRoom);
                _eventBus.Raise(new RoomExitingEvent(currentRoom));
            }

            Room exitingRoom = currentRoom;
            currentRoom.Exit();
            Debug.Log($"Exited room: {exitingRoom.Data.name}");

            currentRoom = null;
        }
    }

    public Room GetCurrentRoom() {
        return currentRoom;
    }

    internal ITargetingService GetActionFiller() {
        throw new NotImplementedException();
    }
}


public class Enemy : Opponent {
    public Func<UniTask> OnSpawned;
    private Speaker speech;
    [Inject] private TurnManager _turnManager;
    [Inject] protected BattleRegistrator opponentRegistrator;

    public Enemy(OpponentData opponentData, DialogueSystem dialogueSystem, GameEventBus eventBus)
        : base(opponentData) {
        SpeechData speechData = opponentData.speechData;
        if (speechData != null) {
            speech = new Speaker(speechData, this, dialogueSystem, eventBus);
        }
    }

    internal async UniTask StartEnemyActivity() {
        if (OnSpawned != null) {
            await OnSpawned.Invoke();
        }

        if (speech != null) {
            await speech.StartDialogue();
        }

        opponentRegistrator.RegisterEnemy(this);
    }

    private async UniTask PerformTestTurn() {
        await UniTask.Delay(1500);
        _turnManager.EndTurnRequest();
    }
}