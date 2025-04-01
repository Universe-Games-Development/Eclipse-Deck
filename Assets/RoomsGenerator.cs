using UnityEditor;
using UnityEngine;
using Zenject;

public class RoomsGenerator : MonoBehaviour
{
    [Header("Debugging")]
    [Inject] DungeonGenerator dungeonGenerator;
    [SerializeField] LocationRoomsData roomsData;
    [SerializeField] DungeonVisualizer visualizer;
    
    public void GenerateTestDungeon() {
        if (dungeonGenerator.GenerateDungeon(roomsData, out DungeonGraph dungeonGraph)) {
            visualizer.VisualizeGraph(dungeonGraph);
        }
    }

    private void Start() {
        GenerateTestDungeon();
    }
}
