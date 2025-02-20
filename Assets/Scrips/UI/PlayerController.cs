using UnityEngine;
using Zenject;

public class PlayerController : MonoBehaviour {
    [Inject] public Player player;
    [Inject] GameboardController gameboard_c;

    [SerializeField] private CardHandUI handUI;
    [SerializeField] private RaycastService rayService;

    protected void Start() {
        handUI.Initialize(player.hand);
    }

    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            Vector3? mouseWorldPosition = rayService.GetRayMousePosition();
            Field field = gameboard_c.GetFieldByWorldPosition(mouseWorldPosition);
            if (field != null) {
                Debug.Log("Clicked Field at: " + field.GetTextCoordinates());
            }
            
        }
    }

}
