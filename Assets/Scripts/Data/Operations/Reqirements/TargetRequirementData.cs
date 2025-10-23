using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RequirementData", menuName = "Requirements/Default")]
public class TargetRequirementData : ScriptableObject {
    [Header("Requirement")]
    private string targetKey; // "Creature", "Zone", etc.
    [SerializeField] public TargetSelector _selector = TargetSelector.Initiator;
    [SerializeField] public bool _allowSameTargetMultipleTimes = false;

    [SerializeReference] public List<TargetConditionData> _targetConditions = new();

    private void OnValidate() {
        if (string.IsNullOrEmpty(targetKey)) {
            targetKey = $"{this} {Guid.NewGuid().ToString()}";
        }
    }
}
