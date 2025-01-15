using UnityEngine;

public abstract class MovementStrategySO : ScriptableObject {
    public abstract IMoveStrategy GetInstance();
}
