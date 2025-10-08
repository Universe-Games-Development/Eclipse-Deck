using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RequirementData", menuName = "Requirements/Default")]
public class RequirementData : ScriptableObject {
    [Header("Requirement")]
    [SerializeField] public string targetKey; // "Creature", "Zone", etc.
    [SerializeField] public TargetSelector _selector = TargetSelector.Initiator;
    [SerializeField] public bool _allowSameTargetMultipleTimes = false;

    [SerializeReference] public List<GlobalConditionData> _globalConditions = new();
    [SerializeReference] public List<TargetConditionData> _targetConditions = new();

    private void OnValidate() {
        if (string.IsNullOrEmpty(targetKey)) {
            targetKey = $"{this} {Guid.NewGuid().ToString()}";
        }
    }
}
