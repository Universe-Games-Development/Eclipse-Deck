using UnityEngine;

public interface ITargetingVisualization {
    void StartTargeting();
    void UpdateTargeting(Vector3 cursorPosition);
    void StopTargeting();
}
