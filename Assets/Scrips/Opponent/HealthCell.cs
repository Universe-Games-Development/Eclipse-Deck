using UnityEngine;

public class HealthCell : MonoBehaviour {
    private void Awake() {

    }

    public void AssignOwner(Opponent opponent) {
        Debug.Log(opponent.Name + " assighned to cell");
    }

    private void OnHealhChanged(int newHealth) {

    }
}
