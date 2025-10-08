using UnityEngine;

[CreateAssetMenu(fileName = "OwnershipConditionData", menuName = "Requirements/Conditions/Ownership")]
public class OwnershipConditionData : TargetConditionData {
    [SerializeField] public OwnershipType ownershipType;
}
