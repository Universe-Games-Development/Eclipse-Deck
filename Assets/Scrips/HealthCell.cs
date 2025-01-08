using UnityEngine;

public class HealthCell : MonoBehaviour
{
    public void AssignOwner(Opponent opponent) {
        Debug.Log(opponent.Name + " assighned to cell");
    }
}
