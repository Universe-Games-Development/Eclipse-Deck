using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BoardSettingsSO))]
public class BoardSettingsEditor : Editor {
    private BoardSettingsSO settings;

    private void OnEnable() {
        settings = (BoardSettingsSO)target;
    }

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        GUILayout.Space(10);
        GUILayout.Label("Grid Controls", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("+ North Row")) settings.AddNorthRow();
        if (GUILayout.Button("- North Row")) settings.RemoveNorthRow();
        if (GUILayout.Button("+ South Row")) settings.AddSouthRow();
        if (GUILayout.Button("- South Row")) settings.RemoveSouthRow();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("+ Column")) settings.AddColumn();
        if (GUILayout.Button("- Column")) settings.RemoveColumn();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Set All Zero")) settings.SetAllGrids(0);
        GUILayout.Space(10);
        if (GUILayout.Button("Set All 1")) settings.SetAllGrids(1);
        GUILayout.Space(10);
        if (GUILayout.Button("Reset Size")) settings.ResetSettings();
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
        DrawGrid("North-West", Direction.NorthWest, settings.northRows);
        DrawGrid("North-East", Direction.NorthEast, settings.northRows);
        DrawGrid("South-West", Direction.SouthWest, settings.southRows);
        DrawGrid("South-East", Direction.SouthEast, settings.southRows);
    }

    private void DrawGrid(string title, Direction dir, int rowCount) {
        if (settings.globalGridData == null) return;

        GUILayout.Label(title, EditorStyles.boldLabel);
        List<List<int>> grid = settings.GetGridDataList(dir);

        // Draw column headers
        GUILayout.BeginHorizontal();
        GUILayout.Label("", GUILayout.Width(30)); // Empty cell for alignment
        for (int j = 0; j < settings.columns; j++) {
            GUILayout.Label($"C {j + 1}", GUILayout.Width(30));
        }
        GUILayout.EndHorizontal();

        // Draw grid with row headers
        for (int i = 0; i < rowCount; i++) {
            if (i >= grid.Count) break; // Захист від виходу за межі
            GUILayout.BeginHorizontal();
            GUILayout.Label($"R {i + 1}", GUILayout.Width(30)); // Row label
            for (int j = 0; j < settings.columns; j++) {
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