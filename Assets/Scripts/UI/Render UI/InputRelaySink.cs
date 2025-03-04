using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InputRelaySink : MonoBehaviour {
    private RectTransform canvasTransform; // Посилання на Canvas
    [SerializeField] private Image debugImage; // Індикатор позиції курсора (для дебагу)

    private GraphicRaycaster raycaster; // Raycaster для обробки UI
    private readonly List<GameObject> dragTargets = new(); // Список об'єктів для перетягування
    private readonly List<RaycastResult> raycastResults = new(); // Результати raycast

    private void Start() {
        raycaster = GetComponent<GraphicRaycaster>();
        if (!canvasTransform) {
            canvasTransform = GetComponent<RectTransform>();
        }
    }

    public void OnCursorInput(Vector2 normalizedPosition) {
        Vector3 mousePosition = CalculateCanvasSpacePosition(normalizedPosition);
        UpdateDebugImagePosition(mousePosition);

        // construct our pointer event
        PointerEventData mouseEvent = new PointerEventData(EventSystem.current);
        mouseEvent.position = mousePosition;

        // perform a raycast using the graphics raycaster
        raycastResults.Clear();

        raycaster.Raycast(mouseEvent, raycastResults);

        bool sendMouseDown = Input.GetMouseButtonDown(0);
        bool sendMouseUp = Input.GetMouseButtonUp(0);
        bool isMouseDown = Input.GetMouseButton(0);

        // send through end drag events as needed
        if (sendMouseUp) {
            foreach (var target in dragTargets) {
                if (ExecuteEvents.Execute(target, mouseEvent, ExecuteEvents.endDragHandler))
                    break;
            }
            dragTargets.Clear();
        }

        // process the raycast results
        foreach (var result in raycastResults) {
            // setup the new event data
            PointerEventData eventData = CreatePointerEventData(mousePosition);
            eventData.pointerCurrentRaycast = eventData.pointerPressRaycast = result;

            // is the mouse down?
            if (isMouseDown)
                eventData.button = PointerEventData.InputButton.Left;

            var slider = result.gameObject.GetComponentInParent<UnityEngine.UI.Slider>();

            // potentially new drag targets?
            if (sendMouseDown) {
                if (ExecuteEvents.Execute(result.gameObject, eventData, ExecuteEvents.beginDragHandler))
                    dragTargets.Add(result.gameObject);

                if (slider != null) {
                    slider.OnInitializePotentialDrag(eventData);

                    if (!dragTargets.Contains(result.gameObject))
                        dragTargets.Add(result.gameObject);
                }
            } // need to update drag target
            else if (dragTargets.Contains(result.gameObject)) {
                eventData.dragging = true;
                ExecuteEvents.Execute(result.gameObject, eventData, ExecuteEvents.dragHandler);
                if (slider != null) {
                    slider.OnDrag(eventData);
                }
            }

            // send a mouse down event?
            if (sendMouseDown) {
                if (ExecuteEvents.Execute(result.gameObject, eventData, ExecuteEvents.pointerDownHandler))
                    break;
            } // send a mouse up event?
            else if (sendMouseUp) {
                bool didRun = ExecuteEvents.Execute(result.gameObject, eventData, ExecuteEvents.pointerUpHandler);
                didRun |= ExecuteEvents.Execute(result.gameObject, eventData, ExecuteEvents.pointerClickHandler);

                if (didRun)
                    break;
            }
        }
    }

    private Vector3 CalculateCanvasSpacePosition(Vector2 normalizedPosition) {
        return new Vector3(
            canvasTransform.sizeDelta.x * normalizedPosition.x,
            canvasTransform.sizeDelta.y * normalizedPosition.y,
            0f
        );
    }

    private void UpdateDebugImagePosition(Vector3 position) {
        if (debugImage) {
            debugImage.rectTransform.anchoredPosition = position - new Vector3(
                canvasTransform.sizeDelta.x / 2,
                canvasTransform.sizeDelta.y / 2,
                0
            );
        }
    }

    private PointerEventData CreatePointerEventData(Vector3 position) {
        return new PointerEventData(EventSystem.current) {
            position = position
        };
    }
}
