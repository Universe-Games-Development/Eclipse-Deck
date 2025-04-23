using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.Splines;
using Zenject;

public class OpponentPresenter : MonoBehaviour, ITargetableObject {
    public Opponent Model { get; protected set; }
    [SerializeField] public OpponentView View;

    [Inject] protected RoomSystem roomSystem;
    public void Initialize(Opponent model) {
        if (model == null) throw new Exception($"Null model for {this}");

        Model = model;

        Model.OnTookSeat += OnTookSeat;
        Model.OnClearedSeat += OnClearSeat;
        Model.OnRoomEntered += OnRoomEntered;
        Model.OnRoomExited += OnRoomExited;
    }

    public async UniTask OnRoomEntered(Room chosenRoom) {
        SplineContainer splineContainer = roomSystem.GetEntrySplineForOpponent(Model, chosenRoom);
        await View.EnterRoom(splineContainer);
    }

    public async UniTask OnRoomExited(Room exitedRoom) {
        SplineContainer splineContainer = roomSystem.GetExitSplineForOpponent(Model, exitedRoom);
        await View.ExitRoom(splineContainer);
    }


    private async UniTask OnTookSeat(BoardSeat seat) {
        await View.TookSeat(seat);
    }

    private async UniTask OnClearSeat() {
        await View.ClearSeat();
    }

    public object GetModel() {
       return Model;
    }
}
