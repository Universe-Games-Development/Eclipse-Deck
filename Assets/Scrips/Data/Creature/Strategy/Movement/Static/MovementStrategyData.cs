using UnityEngine;

public abstract class MovementStrategyData : ScriptableObject {
    public abstract IMoveStrategy GetInstance();
}
