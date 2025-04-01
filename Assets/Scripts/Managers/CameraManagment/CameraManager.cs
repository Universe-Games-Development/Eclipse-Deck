using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using Unity.Cinemachine;
using UnityEngine;
using Zenject;

public class CameraManager : MonoBehaviour {
    public CinemachineCamera mainCamera;

    public CinemachineCamera activeCamera;
    public CinemachineCamera floorCamera;
    [SerializeField] private BoardViews boardViewSwitcher;
    [SerializeField] private CameraSplineMover cameraSplineMover;
    [SerializeField] PlayerPresenter _playerPresenter;

    private GameEventBus _eventBus;
    [Inject]
    public void Construct(GameEventBus eventBus) {
        _eventBus = eventBus;
    }

    void Start() {
        SwitchCamera(floorCamera);
        if (_eventBus != null) {
            _eventBus.SubscribeTo<BattleStartedEvent>(OnBattleStart);
            _eventBus.SubscribeTo<BattleEndEventData>(OnBattleEnd);
        }

        if (_playerPresenter != null && _playerPresenter.Player != null) {
            _playerPresenter.Player.OnRoomEntered += HandleRoomEntered;
            _playerPresenter.Player.OnRoomExited += HandleRoomExited;
        }
    }

    private async UniTask HandleRoomEntered(Room room) {
        // Даємо час поки кімната ініціалізується
        await UniTask.Delay(500);
        SwitchCamera(mainCamera);
    }

    private async UniTask HandleRoomExited(Room room) {
        SwitchCamera(floorCamera);
    }


    private void OnBattleEnd(ref BattleEndEventData eventData) {
        boardViewSwitcher.enabled = false;
        SwitchCamera(mainCamera);
    }

    private void OnBattleStart(ref BattleStartedEvent eventData) {
        boardViewSwitcher.enabled = true;
    }

    public void SwitchCamera(CinemachineCamera newCamera) {
        if (newCamera == null) {
            Debug.LogError($"CameraState {newCamera} is null");
            return;
        }

        if (activeCamera == newCamera) {
            return;
        }

        if (activeCamera != null) {
            activeCamera.Priority = 0;
        }

        activeCamera = newCamera;
        activeCamera.Priority = 1;
    }

    private void OnDestroy() {
        if (_eventBus != null) {
            _eventBus.UnsubscribeFrom<BattleStartedEvent>(OnBattleStart);
            _eventBus.UnsubscribeFrom<BattleEndEventData>(OnBattleEnd);
        }
    }
}
