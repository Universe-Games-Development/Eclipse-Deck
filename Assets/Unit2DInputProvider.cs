using UnityEngine;
using UnityEngine.EventSystems;

public class Unit2DInputProvider : InteractiveUnitInputProviderBase,
                                  IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {
    [SerializeField] private Collider2D myCollider;
    protected override void Awake() {
        base.Awake();
        
    }

    protected override bool InitializeCollider() {
        if (myCollider == null) {
            myCollider = GetComponent<Collider2D>();
        }
        return myCollider != null;
    }

    public void OnPointerClick(PointerEventData eventData) {
        RaiseClicked();
    }

    public void OnPointerEnter(PointerEventData eventData) {
        RaiseCursorEnter();
    }

    public void OnPointerExit(PointerEventData eventData) {
        RaiseCursorExit();
    }

    protected override void UpdateColliderState(bool enabled) {
        myCollider.enabled = enabled;
    }

    
}
