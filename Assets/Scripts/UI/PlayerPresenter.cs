using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
public class PlayerPresenter : BaseOpponentPresenter {
    public new Player Model => (Player)base.Model;
    private new PlayerView View => (PlayerView)base.View;

    public PlayerPresenter(Opponent model, OpponentView view) : base(model, view) {
        if (model is Player player) {
            player.OnRoomEntered += OnRoomEntered;
            player.OnRoomExited += OnRoomExited;
        }
        if (view == null) throw new Exception("Null view for player");
    }

    public async UniTask OnRoomEntered(Room chosenRoom) {
        await View.BeginEntranse();
    }

    public async UniTask OnRoomExited(Room exitedRoom) {
        await View.BeginExiting();
    }
}

public class Player : Opponent {
    public Func<Room, UniTask> OnRoomEntered;
    public Func<Room, UniTask> OnRoomExited;
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

        currentRoom = room;
        Debug.Log($"Room entered: {room.GetName()}");
        room.Enter();
        
    }

    public async UniTask ExitRoom() {
        if (currentRoom != null) {
            if (OnRoomExited != null) {
                await OnRoomExited.Invoke(currentRoom);
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
}


public class BaseOpponentPresenter {
    public Opponent Model { get; protected set; }
    public OpponentView View { get; protected set; }

    public BaseOpponentPresenter(Opponent model, OpponentView view) {
        Model = model;
        View = view;
        Model.OnTookSeat += OnTookSeat;
    }

    private async UniTask OnTookSeat(BoardSeat seat) {
        await View.TookSeat(seat);
    }

    internal ITargetingService GetActionFiller() {
        throw new NotImplementedException();
    }
}