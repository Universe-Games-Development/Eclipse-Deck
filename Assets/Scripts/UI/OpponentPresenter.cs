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
    }

    public async UniTask EnterRoom(Room chosenRoom) {
        SplineContainer splineContainer = roomSystem.GetEntrySplineForOpponent(Model, chosenRoom);
        if (splineContainer == null) return;
        await View.EnterRoom(splineContainer);
    }

    public async UniTask OnRoomExited(Room exitedRoom) {
        SplineContainer splineContainer = roomSystem.GetExitSplineForOpponent(Model, exitedRoom);
        if (splineContainer == null) return;
        await View.ExitRoom(splineContainer);
    }


    public async UniTask MoveToSeat(BoardSeat seat) {
        await View.TookSeat(seat);
    }

    public async UniTask OnClearSeat() {
        await View.ClearSeat();
    }

    public object GetModel() {
       return Model;
    }
}
