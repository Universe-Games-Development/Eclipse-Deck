using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using System.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Splines;
using Zenject;

public class CameraManager : MonoBehaviour {
    public CinemachineCamera activeCamera;

    public CinemachineCamera mainCamera;
    public CinemachineCamera floorCamera;
    public CinemachineCamera dollyCamera;
    [SerializeField] private BoardViews boardViewSwitcher;

    

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
}
