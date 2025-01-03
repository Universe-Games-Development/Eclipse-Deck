using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIZoneHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    [SerializeField] private CinemachineCamera virtualCamera;

    private const int ActivePriority = 10;
    private const int InactivePriority = 0;

    public void OnPointerEnter(PointerEventData eventData) {
        virtualCamera.Priority = ActivePriority;
    }

    public void OnPointerExit(PointerEventData eventData) {
        virtualCamera.Priority = InactivePriority;
    }
}
