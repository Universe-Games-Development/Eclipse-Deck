using UnityEditor;
using UnityEngine;
using Zenject;

public class RoomsGenerator : MonoBehaviour
{
    [Header("Debugging")]
    [Inject] IDungeonGenerator dungeonGenerator;
    [SerializeField] LocationData roomsData;
    [SerializeField] DungeonVisualizer visualizer;
    
    public void GenerateTestDungeon() {
        if (roomsData == null) {
            Debug.LogError("No rooms data provided");
            return;
        }
        if (dungeonGenerator.GenerateDungeon(roomsData, out DungeonGraph dungeonGraph)) {
            visualizer.VisualizeGraph(dungeonGraph);
        }
    }

    private void Start() {
        GenerateTestDungeon();
    }
}
