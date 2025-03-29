using UnityEngine;

public class RoomView : MonoBehaviour {
    private GameObject currentView;
    [SerializeField] private Transform modelParent;
    public void InitializeView(RoomData roomData) {
        if (currentView != null) {
            Destroy(currentView);
        }
        currentView = Instantiate(roomData.ViewPrefab);
    }
}
