using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "OperationData", menuName = "Operations/Default")]
public class OperationData : ScriptableObject {
    [SerializeReference] public List<RequirementData> requirements = new();
    [SerializeReference] public VisualData visualData;
}

public class GlobalConditionData : ScriptableObject {

}
