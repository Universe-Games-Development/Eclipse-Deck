using UnityEngine;
using Zenject;

public class TurnButton : MonoBehaviour {
    [Inject] protected UIManager uiManager;

    void OnMouseEnter() {
        uiManager.ShowTip("Turn Button");
    }

    private void OnMouseUpAsButton() {
        Debug.Log("Turn changed!");
    }
}
