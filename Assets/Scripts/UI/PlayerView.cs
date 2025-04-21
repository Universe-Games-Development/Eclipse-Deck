using Cysharp.Threading.Tasks;
using UnityEngine;

public class PlayerView : OpponentView {
    [SerializeField] private CameraManager cameraManager;
    public async UniTask BeginEntranse() {
        await cameraManager.BeginEntranse();
    }

    public async UniTask BeginExiting() {
        await cameraManager.BeginExiting();
    }
}
