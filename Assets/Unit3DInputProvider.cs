public class Unit3DInputProvider : InteractiveUnitInputProviderBase {
    private void OnMouseDown() {
        RaiseClicked();
    }

    private void OnMouseEnter() {
        RaiseCursorEnter();
    }

    private void OnMouseExit() {
        RaiseCursorExit();
    }
}
