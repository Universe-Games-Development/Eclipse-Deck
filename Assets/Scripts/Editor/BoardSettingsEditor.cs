using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BoardSettingsData))]
public class BoardSettingsEditor : Editor {
    private BoardSettingsData settings;

    private void OnEnable() {
        if (target != null)
            settings = (BoardSettingsData)target;
    }

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        GUILayout.Space(10);
        GUILayout.Label("Grid Controls", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("+ North Row")) settings.AddRow(Direction.North);
        if (GUILayout.Button("- North Row")) settings.RemoveRow(Direction.North);
        if (GUILayout.Button("+ South Row")) settings.AddRow(Direction.North);
        if (GUILayout.Button("- South Row")) settings.RemoveRow(Direction.South);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("+ West Column")) settings.AddColumn(Direction.West);
        if (GUILayout.Button("- West Column")) settings.RemoveColumn(Direction.West);
        if (GUILayout.Button("+ East Column")) settings.AddColumn(Direction.East);
        if (GUILayout.Button("- East Column")) settings.RemoveColumn(Direction.East);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Set All Zero")) settings.SetAllGrids(0);
        GUILayout.Space(10);
        if (GUILayout.Button("Set All 1")) settings.SetAllGrids(1);
        GUILayout.Space(10);
        if (GUILayout.Button("Reset Size")) settings.ResetGrids();
        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        if (GUILayout.Button("Randomize All")) settings.RandomizeAllGrids();
        GUILayout.Space(10);

        GUILayout.Label("Edit Grid Values", EditorStyles.boldLabel);

        ReDrawGrids();

        if (GUI.changed) {
            EditorUtility.SetDirty(settings);
        }
    }

    private void ReDrawGrids() {
        DrawGrid("North-West", Direction.NorthWest, settings.northRows, settings.westColumns);
        DrawGrid("North-East", Direction.NorthEast, settings.northRows, settings.eastColumns);
        DrawGrid("South-West", Direction.SouthWest, settings.southRows, settings.westColumns);
        DrawGrid("South-East", Direction.SouthEast, settings.southRows, settings.eastColumns);
    }

    private void DrawGrid(string title, Direction dir, int rowCount, int columnsCount) {
        if (!settings.IsInitialized()) return;

        GUILayout.Label(title, EditorStyles.boldLabel);
        List<List<int>> grid = settings.GetGridValues(dir);

        // Draw column headers
        GUILayout.BeginHorizontal();
        GUILayout.Label("", GUILayout.Width(30)); // Empty cell for alignment
        for (int j = 0; j < columnsCount; j++) {
            GUILayout.Label($"C {j + 1}", GUILayout.Width(30));
        }
        GUILayout.EndHorizontal();

        // Draw grid with row headers
        for (int i = 0; i < rowCount; i++) {
            if (i >= grid.Count) break; // Захист від виходу за межі
            GUILayout.BeginHorizontal();
            GUILayout.Label($"R {i + 1}", GUILayout.Width(30)); // Row label
            for (int j = 0; j < columnsCount; j++) {
                if (j >= grid[i].Count) break;
                grid[i][j] = EditorGUILayout.IntField(grid[i][j], GUILayout.Width(30));
            }
            GUILayout.EndHorizontal();
        }

        if (GUILayout.Button("Randomize", GUILayout.Width(120))) {
            settings.RandomizeGrid(dir);
        }
        GUILayout.Space(5);
    }

}