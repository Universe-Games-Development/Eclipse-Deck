public enum AttackMovementStrategyType {
    None, // stays
    SimpleMove, // goes left 
    TraverseRight, // goes right 
    Retreat, // Retreats if against enemy
    ScaredRetreat, // Retreats if against is more stronger enemy
    AloneScared, // retreats if there is no ally on sides
    Hero, // Can`t fight together with allies on sides
}

public enum SupportMovementStrategyType {
    None, // stays
    SimpleMove, // moves forward
}
