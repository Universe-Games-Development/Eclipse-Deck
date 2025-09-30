using UnityEngine.EventSystems;

public class Unit2DInputProvider : InteractiveUnitInputProviderBase,
                                  IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {
    public void OnPointerClick(PointerEventData eventData) {
        RaiseClicked();
    }

    public void OnPointerEnter(PointerEventData eventData) {
        RaiseCursorEnter();
    }

    public void OnPointerExit(PointerEventData eventData) {
        RaiseCursorExit();
    }
}
