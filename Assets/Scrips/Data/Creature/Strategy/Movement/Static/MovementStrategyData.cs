using UnityEngine;

public abstract class MovementStrategyData : ScriptableObject {
    public abstract MovementStrategy GetInstance();
}
