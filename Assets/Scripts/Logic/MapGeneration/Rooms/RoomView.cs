using UnityEngine;
using UnityEngine.Splines;

public class RoomView : MonoBehaviour {
    internal SplineContainer playerEntrySpline;
    internal SplineContainer enemyEntrySpline;
    internal SplineContainer playerExitSpline;
    internal SplineContainer enemyExitSpline;
    private GameObject currentView;
    [SerializeField] private Transform modelParent;
    public void InitializeView(RoomData roomData) {
        if (currentView != null) {
            Destroy(currentView);
        }
        currentView = Instantiate(roomData.ViewPrefab);
    }
}
