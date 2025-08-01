using UnityEngine;

public class NamedTarget {
    public string targetKey;
    public IRequirement requirement;
    public GameObject selectedObject;
    public bool IsFilled = false;

    public NamedTarget(string targetName, IRequirement requirement) {
        this.targetKey = targetName;
        this.requirement = requirement;
    }
}
