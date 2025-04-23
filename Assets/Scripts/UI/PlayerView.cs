using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Splines;

public class PlayerView : OpponentView {
    [SerializeField] private CameraManager cameraManager;

    public override async UniTask EnterRoom(SplineContainer splineContainer) {
        cameraManager.SwitchCamera(cameraManager.dollyCamera);
        await MoveAlongSpline(splineContainer);
        cameraManager.SwitchCamera(cameraManager.mainCamera);
    }

    public override async UniTask ExitRoom(SplineContainer splineContainer) {
        await MoveAlongSpline(splineContainer);
        cameraManager.SwitchCamera(cameraManager.floorCamera);
    }
}