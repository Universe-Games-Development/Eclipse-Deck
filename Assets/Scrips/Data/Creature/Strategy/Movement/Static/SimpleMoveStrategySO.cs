using System.Collections.Generic;
using UnityEngine;

/* The logic of movements for creature:
 * 1. I'm go on X tiles
 */
[CreateAssetMenu(fileName = "SimpleMoveSO", menuName = "Strategies/Movement/Simple")]
public class SimpleMoveStrategySO : StaticMovementStrategySO {
    public int defaultMoveAmount = 1;
    public Direction defaultMoveDirection = Direction.South;

    protected override List<Path> Move() {
        List<Path> paths = new() {
            navigator.GenerateSimplePath(defaultMoveAmount, defaultMoveDirection)
        };
        return paths;
    }
}