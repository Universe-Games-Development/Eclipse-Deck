/* The logic of movements for creature:
 * 1. I'm do nothing
 */
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NoneMoveStrategySO", menuName = "Strategies/Movement/None")]
public class NoneMoveStrategySO : StaticMovementStrategySO {
    protected override List<Path> Move() {
        List<Path> paths = new();
        return paths;
    }
}