using UnityEngine;

public class ResourcesDebugger : MonoBehaviour {
    [SerializeField]
    private string resourcePath = ""; // ������� �������, ��� ��������� ���� Resources

    public void DebugResources() {
        Object[] allResources = Resources.LoadAll(resourcePath);
        if (allResources.Length > 0) {
            Debug.Log($"Found {allResources.Length} resources in path '{resourcePath}':");
            foreach (var resource in allResources) {
                Debug.Log($"- {resource.name} ({resource.GetType()})");
            }
        } else {
            Debug.LogWarning($"No resources found in path: {resourcePath}");
        }
    }

    // ������ ����� Unity Editor
    [ContextMenu("Debug Resources")]
    public void DebugResourcesFromEditor() {
        DebugResources();
    }
}
