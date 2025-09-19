using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.Splines;
using Zenject;

public class CharacterPresenter : MonoBehaviour, ITargetableObject {
    public Opponent Opponent { get; protected set; }
    [SerializeField] public CharacterView View;

    [Inject] protected RoomSystem roomSystem;
    public void Initialize(Opponent model) {
        if (model == null) throw new Exception($"Null model for {this}");

        Opponent = model;
    }

    public async UniTask EnterRoom(Room chosenRoom) {
        SplineContainer splineContainer = roomSystem.GetEntrySplineForOpponent(Opponent, chosenRoom);
        if (splineContainer == null) return;
        await View.EnterRoom(splineContainer);
    }

    public async UniTask OnRoomExited(Room exitedRoom) {
        SplineContainer splineContainer = roomSystem.GetExitSplineForOpponent(Opponent, exitedRoom);
        if (splineContainer == null) return;
        await View.ExitRoom(splineContainer);
    }


    public async UniTask MoveToSeat(Transform destination) {
        await View.MoveToTransform(destination);
    }

    public async UniTask OnClearSeat() {
        await View.ClearSeat();
    }

    public object GetModel() {
        return Opponent;
    }
}
