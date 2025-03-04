using UnityEngine;
using Zenject;
using Zenject.SpaceFighter;

public class PlayerController : MonoBehaviour {
    [Inject] public Player player;
    [Inject] GameBoardController gameboard_c;
    [Inject] DiContainer container;
    

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
