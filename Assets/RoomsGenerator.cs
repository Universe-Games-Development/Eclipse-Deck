using UnityEditor;
using UnityEngine;
using Zenject;

public class RoomsGenerator : MonoBehaviour
{
    [Header("Debugging")]
    [Inject] IDungeonGenerator dungeonGenerator;
    [SerializeField] LocationData locationData;
    [SerializeField] DungeonVisualizer visualizer;
    
    public void GenerateTestDungeon() {
        if (locationData == null) {
            Debug.LogError("No rooms data provided");
            return;
        }
        if (dungeonGenerator.GenerateDungeon(locationData, out DungeonGraph dungeonGraph)) {
            visualizer.VisualizeGraph(dungeonGraph);
        }
    }

    private void Start() {
        GenerateTestDungeon();
    }
}
