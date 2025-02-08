using UnityEngine;
using UnityEngine.Events;

public class InputRelaySource : MonoBehaviour {

    public UnityEvent<Vector2> OnCursorInput = new();

    [Header("Raycasting")]
    [SerializeField] private LayerMask renderTextureLayer;
    [SerializeField] private float rayDistance = 50f;

    private void Update() {
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(mouseRay, out RaycastHit hit, rayDistance, renderTextureLayer)) {
            OnCursorInput.Invoke(hit.textureCoord);
        }
    }
}
