using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// Клас для представлення окремої клітинки
[Serializable]
public class HexCell {
    public Vector2Int coordinates;
    public bool exists = true;
    public int terrainType = 0; // Можна розширити для різних типів місцевості

    // Додаткові дані, які можуть бути потрібні для гри
    public int elevation = 0;
    public bool isWalkable = true;

    public HexCell(int x, int y) {
        coordinates = new Vector2Int(x, y);
    }

    public HexCell Clone() {
        HexCell newCell = new HexCell(coordinates.x, coordinates.y);
        newCell.exists = exists;
        newCell.terrainType = terrainType;
        newCell.elevation = elevation;
        newCell.isWalkable = isWalkable;
        return newCell;
    }
}

// Основний клас для роботи з гексагональною сіткою
public class HexGrid {
    private Dictionary<Vector2Int, HexCell> cells = new Dictionary<Vector2Int, HexCell>();
    public int width { get; private set; }
    public int height { get; private set; }
    public float hexSize { get; set; } = 1f;

    public HexGrid(int width, int height) {
        this.width = width;
        this.height = height;
        Initialize();
    }

    public void Initialize() {
        cells.Clear();
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                HexCell cell = new HexCell(x, y);
                cells[new Vector2Int(x, y)] = cell;
            }
        }
    }

    public HexCell GetCell(int x, int y) {
        Vector2Int coords = new Vector2Int(x, y);
        if (cells.TryGetValue(coords, out HexCell cell)) {
            return cell;
        }
        return null;
    }

    public bool SetCellExists(int x, int y, bool exists) {
        HexCell cell = GetCell(x, y);
        if (cell != null && cell.exists != exists) {
            cell.exists = exists;
            return true; // Зміна стану клітинки
        }
        return false;
    }

    public Vector3 GetHexPosition(int x, int y) {
        // Формула для позиції гексагона в гексагональній сітці
        float xPos = x * hexSize * 1.5f;
        float zPos = y * hexSize * Mathf.Sqrt(3f) + (x % 2 == 1 ? hexSize * Mathf.Sqrt(3f) / 2f : 0);

        return new Vector3(xPos, 0, zPos);
    }

    public List<Vector2Int> GetHexNeighbors(int x, int y) {
        List<Vector2Int> neighbors = new List<Vector2Int>();

        // Зміщення для парних рядків (x)
        int[][] evenOffsets = new int[][]
        {
            new int[] { 1, 0 },   // Праворуч
            new int[] { 0, 1 },   // Верх-праворуч
            new int[] { -1, 1 },  // Верх-ліворуч
            new int[] { -1, 0 },  // Ліворуч
            new int[] { -1, -1 }, // Низ-ліворуч
            new int[] { 0, -1 }   // Низ-праворуч
        };

        // Зміщення для непарних рядків (x)
        int[][] oddOffsets = new int[][]
        {
            new int[] { 1, 0 },   // Праворуч
            new int[] { 1, 1 },   // Верх-праворуч
            new int[] { 0, 1 },   // Верх-ліворуч
            new int[] { -1, 0 },  // Ліворуч
            new int[] { 0, -1 },  // Низ-ліворуч
            new int[] { 1, -1 }   // Низ-праворуч
        };

        int[][] offsets = x % 2 == 0 ? evenOffsets : oddOffsets;

        foreach (int[] offset in offsets) {
            int nx = x + offset[0];
            int ny = y + offset[1];

            HexCell neighborCell = GetCell(nx, ny);
            if (neighborCell != null && neighborCell.exists) {
                neighbors.Add(new Vector2Int(nx, ny));
            }
        }

        return neighbors;
    }

    // Методи для операцій з сіткою
    public void FillAll(bool value) {
        foreach (var cell in cells.Values) {
            cell.exists = value;
        }
    }

    public void CreateCheckerPattern() {
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                HexCell cell = GetCell(x, y);
                if (cell != null) {
                    cell.exists = (x + y) % 2 == 0;
                }
            }
        }
    }

    public void CreateBorderPattern() {
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                HexCell cell = GetCell(x, y);
                if (cell != null) {
                    bool isBorder = x == 0 || y == 0 || x == width - 1 || y == height - 1;
                    cell.exists = isBorder;
                }
            }
        }
    }

    public void CreateCirclePattern() {
        // Знаходимо центр сітки
        float centerX = (width - 1) / 2.0f;
        float centerY = (height - 1) / 2.0f;

        // Визначаємо радіус як 40% від розміру сітки
        float maxRadius = Mathf.Min(width, height) * 0.4f;

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                HexCell cell = GetCell(x, y);
                if (cell != null) {
                    // Враховуємо зміщення для непарних рядків при обчисленні відстані
                    float offsetY = 0;
                    if (x % 2 == 1) {
                        offsetY = 0.5f;
                    }

                    // Обчислюємо відстань до центру
                    float dx = x - centerX;
                    float dy = (y + offsetY) - centerY;

                    // Коефіцієнт корекції для гексагональної сітки
                    float distance = Mathf.Sqrt(dx * dx * 0.75f + dy * dy);

                    cell.exists = distance <= maxRadius;
                }
            }
        }
    }

    public Dictionary<Vector2Int, HexCell> GetAllCells() {
        return cells;
    }

    // Клонування сітки для безпечної роботи
    public HexGrid Clone() {
        HexGrid newGrid = new HexGrid(width, height);
        newGrid.hexSize = this.hexSize;

        foreach (var kvp in cells) {
            newGrid.cells[kvp.Key] = kvp.Value.Clone();
        }

        return newGrid;
    }

    public bool HasAnyData() {
        return cells.Count > 0;
    }
}

[CreateAssetMenu(fileName = "HexGridData", menuName = "TGE/Board/Hex Grid Data", order = 1)]
public class HexGridData : ScriptableObject {
    public int width = 10;
    public int height = 10;
    public float hexSize = 1f;

    // Серіалізовані дані клітинок
    [SerializeField, HideInInspector]
    private CellData[] serializedCells;

    // Тимчасова робоча сітка для редактора
    [NonSerialized]
    private HexGrid workingGrid;

    [Serializable]
    private class CellData {
        public int x;
        public int y;
        public bool exists;
        public int terrainType;
        public int elevation;
        public bool isWalkable;

        public CellData(global::HexCell cell) {
            x = cell.coordinates.x;
            y = cell.coordinates.y;
            exists = cell.exists;
            terrainType = cell.terrainType;
            elevation = cell.elevation;
            isWalkable = cell.isWalkable;
        }
    }

    public void OnEnable() {
        // Завантажуємо дані в робочу сітку при завантаженні ScriptableObject
        LoadWorkingGrid();
    }

    // Метод для отримання робочого HexGrid
    public HexGrid GetWorkingGrid() {
        if (workingGrid == null) {
            LoadWorkingGrid();
        }
        return workingGrid;
    }

    // Завантаження даних в робочу сітку
    private void LoadWorkingGrid() {
        workingGrid = new HexGrid(width, height);
        workingGrid.hexSize = hexSize;

        if (serializedCells != null && serializedCells.Length > 0) {
            foreach (var cellData in serializedCells) {
                global::HexCell cell = workingGrid.GetCell(cellData.x, cellData.y);
                if (cell != null) {
                    cell.exists = cellData.exists;
                    cell.terrainType = cellData.terrainType;
                    cell.elevation = cellData.elevation;
                    cell.isWalkable = cellData.isWalkable;
                }
            }
        }
    }

    // Збереження робочої сітки в ScriptableObject
    public void SaveWorkingGrid() {
        if (workingGrid == null) return;

        // Отримуємо всі клітинки
        var cells = workingGrid.GetAllCells();
        serializedCells = new CellData[cells.Count];

        int index = 0;
        foreach (var cell in cells.Values) {
            serializedCells[index] = new CellData(cell);
            index++;
        }
    }

    // Метод для оновлення параметрів сітки
    public void UpdateGridParameters(int newWidth, int newHeight, float newHexSize) {
        width = newWidth;
        height = newHeight;
        hexSize = newHexSize;

        // Створюємо нову робочу сітку з новими параметрами
        workingGrid = new HexGrid(width, height);
        workingGrid.hexSize = hexSize;

        // Зберігаємо зміни
        SaveWorkingGrid();
    }

    // Публічний метод для оновлення даних з будь-якої сітки
    public void UpdateFromGrid(HexGrid grid) {
        width = grid.width;
        height = grid.height;
        hexSize = grid.hexSize;

        workingGrid = grid.Clone();
        SaveWorkingGrid();
    }

    // Отримати Unity-позицію для гексагона
    public Vector3 GetHexPosition(int x, int y) {
        return GetWorkingGrid().GetHexPosition(x, y);
    }

    // Отримати сусідів гексагона
    public List<Vector2Int> GetHexNeighbors(int x, int y) {
        return GetWorkingGrid().GetHexNeighbors(x, y);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(HexGridData))]
public class HexGridDataEditor : Editor {
    // Константи для розрахунків сітки
    private const float DEFAULT_HEX_WIDTH = 30f;
    private const float DEFAULT_HEX_HEIGHT = 30f;
    private const float UI_VERTICAL_OFFSET = 230f;
    private const float GRID_OFFSET_X = 20f;
    private const float GRID_OFFSET_Y = 20f;

    // Основні дані
    private HexGridData gridData;
    private HexGrid workingGrid;
    private bool isEditing = false;
    private Vector2 scrollPosition;

    // Інструменти редагування
    private enum EditingTool { SingleCell, Brush, Line }
    private EditingTool selectedTool = EditingTool.SingleCell;
    private int brushSize = 1;

    // UI стан
    private Vector2Int? hoveredCell = null;
    private bool showCoordinates = true;
    private Vector2 lastMousePosition;

    // Стилі
    private GUIStyle coordinateStyle;
    private GUIStyle tooltipStyle;

    // Кешування для оптимізації
    private Dictionary<Vector2Int, Vector2> cellCenterPositions = new Dictionary<Vector2Int, Vector2>();
    private bool gridLayoutChanged = true;

    private void OnEnable() {
        gridData = (HexGridData)target;
        workingGrid = gridData.GetWorkingGrid();
        InitializeStyles();
    }

    private void InitializeStyles() {
        // Стиль для координат
        coordinateStyle = new GUIStyle {
            normal = { textColor = Color.black },
            fontSize = 10,
            alignment = TextAnchor.MiddleCenter
        };

        // Стиль для підказок
        tooltipStyle = new GUIStyle {
            normal = {
                textColor = Color.white,
                background = CreateSolidTexture(2, 2, new Color(0.1f, 0.1f, 0.1f, 0.8f))
            },
            fontSize = 12,
            padding = new RectOffset(5, 5, 5, 5)
        };
    }

    private Texture2D CreateSolidTexture(int width, int height, Color color) {
        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = color;

        Texture2D texture = new Texture2D(width, height);
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();

        // Малювання основних параметрів сітки
        DrawGridSettings();

        // Кнопка для переключення режиму редагування
        DrawEditModeToggle();

        // Редактор сітки
        if (isEditing && workingGrid != null) {
            DrawGridEditor();
        }

        if (EditorGUI.EndChangeCheck()) {
            serializedObject.ApplyModifiedProperties();
        }
    }

    private void DrawGridSettings() {
        EditorGUILayout.LabelField("Grid Settings", EditorStyles.boldLabel);

        int newWidth = EditorGUILayout.IntField("Width", gridData.width);
        int newHeight = EditorGUILayout.IntField("Height", gridData.height);
        float newHexSize = EditorGUILayout.FloatField("Hex Size", gridData.hexSize);

        // Перевіряємо чи змінився розмір сітки
        if (newWidth != gridData.width || newHeight != gridData.height || newHexSize != gridData.hexSize) {
            HandleGridSizeChange(newWidth, newHeight, newHexSize);
        }

        EditorGUILayout.Space();
    }

    private void HandleGridSizeChange(int newWidth, int newHeight, float newHexSize) {
        // Запитуємо підтвердження тільки якщо сітка вже містить дані
        bool shouldReset = true;
        if (workingGrid != null && workingGrid.HasAnyData()) {
            shouldReset = EditorUtility.DisplayDialog(
                "Reset Grid?",
                "Changing grid dimensions will reset all hex data. Continue?",
                "Yes", "No");
        }

        if (shouldReset) {
            gridData.UpdateGridParameters(
                Mathf.Max(1, newWidth),
                Mathf.Max(1, newHeight),
                Mathf.Max(0.1f, newHexSize)
            );

            // Оновлюємо робочу сітку і відмічаємо, що розміщення змінилось
            workingGrid = gridData.GetWorkingGrid();
            gridLayoutChanged = true;
            EditorUtility.SetDirty(gridData);
        }
    }

    private void DrawEditModeToggle() {
        if (GUILayout.Button(isEditing ? "Exit Edit Mode" : "Edit Hex Grid", GUILayout.Height(30))) {
            isEditing = !isEditing;

            // При виході з режиму редагування зберігаємо зміни
            if (!isEditing && workingGrid != null) {
                gridData.UpdateFromGrid(workingGrid);
                EditorUtility.SetDirty(gridData);
            }
        }
    }

    private void DrawGridEditor() {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Hex Grid Editor", EditorStyles.boldLabel);

        // Інструменти редагування
        DrawToolbar();

        showCoordinates = EditorGUILayout.Toggle("Show Coordinates", showCoordinates);
        EditorGUILayout.HelpBox("Left-click to enable, right-click to disable hexes", MessageType.Info);

        // Прокрутка для великих сіток
        Rect totalRect = EditorGUILayout.GetControlRect(false, 400);
        Rect viewRect = new Rect(0, 0, totalRect.width - 20,
            CalculateGridHeight(gridData.width, gridData.height) + 50);

        scrollPosition = GUI.BeginScrollView(totalRect, scrollPosition, viewRect);

        // Перебудовуємо кеш позицій клітинок, якщо розмір сітки змінився
        if (gridLayoutChanged) {
            RecalculateCellPositions();
            gridLayoutChanged = false;
        }

        // Малювання гексагональної сітки
        DrawHexGrid(viewRect);

        GUI.EndScrollView();

        DrawGridActions();

        // Якщо змінилась позиція миші над редактором, перевіряємо клітинку під курсором
        ProcessMouseInput();

        // Перемальовуємо вікно редактора для відображення підсвітки в реальному часі
        if (selectedTool != EditingTool.SingleCell)
            Repaint();
    }

    private void DrawToolbar() {
        string[] toolNames = { "Single Cell", "Brush", "Line" };
        int toolIndex = (int)selectedTool;
        int newToolIndex = GUILayout.Toolbar(toolIndex, toolNames);

        if (newToolIndex != toolIndex) {
            selectedTool = (EditingTool)newToolIndex;
        }

        // Налаштування інструментів
        EditorGUILayout.BeginHorizontal();

        if (selectedTool == EditingTool.Brush) {
            EditorGUILayout.LabelField("Brush Size:", GUILayout.Width(70));
            brushSize = EditorGUILayout.IntSlider(brushSize, 1, 5);
        }

        EditorGUILayout.EndHorizontal();
    }

    private void DrawGridActions() {
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Fill All")) {
            workingGrid.FillAll(true);
            SaveGridChanges();
        }

        if (GUILayout.Button("Clear All")) {
            workingGrid.FillAll(false);
            SaveGridChanges();
        }

        EditorGUILayout.EndHorizontal();

        // Додаткові паттерни гексів
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Preset Patterns", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Checker Pattern")) {
            workingGrid.CreateCheckerPattern();
            SaveGridChanges();
        }

        if (GUILayout.Button("Border Only")) {
            workingGrid.CreateBorderPattern();
            SaveGridChanges();
        }

        if (GUILayout.Button("Circle/Oval")) {
            workingGrid.CreateCirclePattern();
            SaveGridChanges();
        }

        EditorGUILayout.EndHorizontal();
    }

    private void SaveGridChanges() {
        gridData.UpdateFromGrid(workingGrid);
        EditorUtility.SetDirty(gridData);
    }

    private float CalculateGridHeight(int width, int height) {
        // Розрахунок висоти сітки для правильного розміру прокрутки
        float hexHeight = DEFAULT_HEX_HEIGHT;
        float verticalSpacing = hexHeight * 0.75f;

        return height * verticalSpacing + hexHeight + GRID_OFFSET_Y * 2;
    }

    private void RecalculateCellPositions() {
        cellCenterPositions.Clear();

        float hexWidth = DEFAULT_HEX_WIDTH;
        float hexHeight = DEFAULT_HEX_HEIGHT;
        float horizontalSpacing = hexWidth * 0.75f;
        float verticalSpacing = hexHeight * 0.75f;

        for (int y = 0; y < gridData.height; y++) {
            for (int x = 0; x < gridData.width; x++) {
                // Зміщення для непарних рядків x
                float rowOffset = x % 2 == 1 ? verticalSpacing / 2 : 0;

                // Позиція центру гексагона
                float hexX = GRID_OFFSET_X + x * horizontalSpacing;
                float hexY = GRID_OFFSET_Y + y * verticalSpacing + rowOffset;

                cellCenterPositions[new Vector2Int(x, y)] = new Vector2(hexX, hexY);
            }
        }
    }

    private void ProcessMouseInput() {
        Event e = Event.current;

        // Пропускаємо обробку, якщо миша не рухається або не натиснуті кнопки
        if (e.type != EventType.MouseDown && e.type != EventType.MouseDrag &&
            e.type != EventType.MouseMove && lastMousePosition == e.mousePosition) {
            return;
        }

        lastMousePosition = e.mousePosition;

        // Знаходимо координати гекса під курсором
        hoveredCell = GetHexCoordinateAtScreenPosition(e.mousePosition);

        // Обробляємо кліки миші
        if ((e.type == EventType.MouseDown || e.type == EventType.MouseDrag) &&
            hoveredCell.HasValue && (e.button == 0 || e.button == 1)) {
            bool setValue = (e.button == 0); // Ліва кнопка = true, права = false
            Vector2Int cell = hoveredCell.Value;

            // Тут можлива пакетна обробка змін для оптимізації
            bool gridChanged = false;

            switch (selectedTool) {
                case EditingTool.SingleCell:
                    gridChanged = workingGrid.SetCellExists(cell.x, cell.y, setValue);
                    break;

                case EditingTool.Brush:
                    gridChanged = ApplyBrush(cell, setValue);
                    break;

                case EditingTool.Line:
                    gridChanged = workingGrid.SetCellExists(cell.x, cell.y, setValue);
                    break;
            }

            // Зберігаємо зміни тільки якщо реально були зміни
            if (gridChanged) {
                // Відмічаємо що дані змінились, але не зберігаємо на кожній зміні
                // Це зменшить навантаження при малюванні великим пензлем
                if (e.type == EventType.MouseUp) {
                    SaveGridChanges();
                }
            }

            // Повідомляємо Unity що ми змінили дані
            e.Use();
        }
    }

    private bool ApplyBrush(Vector2Int center, bool value) {
        bool anyChanged = false;

        // Застосовуємо значення до центральної клітинки та всіх в радіусі brushSize
        List<Vector2Int> cellsToChange = HexUtils.GetCellsInRadiusOffset(center, brushSize, gridData.width, gridData.height);

        foreach (Vector2Int cell in cellsToChange) {
            bool changed = workingGrid.SetCellExists(cell.x, cell.y, value);
            anyChanged |= changed;
        }

        return anyChanged;
    }

    private Vector2Int? GetHexCoordinateAtScreenPosition(Vector2 inputPos) {
        // Оптимізація: замість перебору всіх клітинок, спочатку перевіряємо, чи знаходиться
        // курсор взагалі в межах сітки

        Vector2 adjustedPosition = new Vector2(
            inputPos.x + scrollPosition.x,
            inputPos.y - UI_VERTICAL_OFFSET + scrollPosition.y
        );

        // Якщо grid layout змінився, перераховуємо позиції
        if (gridLayoutChanged) {
            RecalculateCellPositions();
            gridLayoutChanged = false;
        }

        // Розмір гексагона для перевірки попадання
        float hexRadius = DEFAULT_HEX_WIDTH / 2;

        // Знаходимо найближчу клітинку (оптимізація замість повного перебору)
        Vector2Int? closestCell = null;
        float minDistance = float.MaxValue;

        foreach (var cellPos in cellCenterPositions) {
            float distance = Vector2.Distance(adjustedPosition, cellPos.Value);
            if (distance < minDistance) {
                minDistance = distance;
                closestCell = cellPos.Key;
            }
        }

        // Перевіряємо, чи знаходиться точка в межах гексагона
        if (closestCell.HasValue && minDistance <= hexRadius * 1.15f) // Невеликий запас для зручності
        {
            return closestCell;
        }

        return null;
    }

    private void DrawHexGrid(Rect viewRect) {
        float hexRadius = DEFAULT_HEX_WIDTH / 2;

        // Малюємо гексагони використовуючи кешовані позиції
        foreach (var cellEntry in cellCenterPositions) {
            Vector2Int cellCoord = cellEntry.Key;
            Vector2 cellCenter = cellEntry.Value;

            if (cellCoord.x < 0 || cellCoord.x >= gridData.width ||
                cellCoord.y < 0 || cellCoord.y >= gridData.height)
                continue;

            // Отримуємо клітинку
            HexCell cell = workingGrid.GetCell(cellCoord.x, cellCoord.y);
            if (cell == null) continue;

            // Колір гексагона залежить від його стану
            Color fillColor = cell.exists ?
                new Color(0.7f, 0.9f, 0.7f) :
                new Color(0.9f, 0.7f, 0.7f);

            // Підсвічуємо клітинку під курсором
            if (hoveredCell.HasValue && hoveredCell.Value.Equals(cellCoord)) {
                // Додаємо підсвітку для клітинки під курсором
                fillColor = Color.Lerp(fillColor, Color.yellow, 0.3f);
            }

            // Малюємо гексагон
            DrawHexagon(cellCenter.x, cellCenter.y, hexRadius, fillColor, false);

            // Малюємо координати, якщо опція включена
            if (showCoordinates) {
                Rect labelRect = new Rect(cellCenter.x - 15, cellCenter.y - 8, 30, 16);
                GUI.Label(labelRect, $"{cellCoord.x},{cellCoord.y}", coordinateStyle);
            }
        }

        // Малюємо підказку про координати під курсором
        if (hoveredCell.HasValue) {
            Vector2 mousePos = Event.current.mousePosition;
            Rect tooltipRect = new Rect(mousePos.x + 15, mousePos.y + 15, 70, 25);
            GUI.Label(tooltipRect, $"{hoveredCell.Value.x},{hoveredCell.Value.y}", tooltipStyle);

            // Для інструмента "Brush" показуємо також радіус дії
            if (selectedTool == EditingTool.Brush && brushSize > 1) {
                Vector2Int center = hoveredCell.Value;
                List<Vector2Int> brushCells = HexUtils.GetCellsInRadiusOffset(center, brushSize, gridData.width, gridData.height);

                foreach (Vector2Int cell in brushCells) {
                    if (cellCenterPositions.TryGetValue(cell, out Vector2 pos)) {
                        // Малюємо прозоре підсвічування
                        DrawHexagon(pos.x, pos.y, hexRadius, new Color(1f, 1f, 0f, 0.2f), true);
                    }
                }
            }
        }
    }

    private void DrawHexagon(float x, float y, float size, Color color, bool outline) {
        // Вершини гексагона
        Vector3[] vertices = new Vector3[6];
        for (int i = 0; i < 6; i++) {
            float angle = 2 * Mathf.PI / 6 * i;
            vertices[i] = new Vector2(
                x + size * Mathf.Cos(angle),
                y + size * Mathf.Sin(angle)
            );
        }

        // Малюємо гексагон
        Handles.BeginGUI();

        if (!outline) {
            // Заповнений гексагон
            Handles.color = color;
            Handles.DrawAAConvexPolygon(vertices);

            // Контур гексагона
            Handles.color = Color.black;
            for (int i = 0; i < 6; i++) {
                Handles.DrawLine(vertices[i], vertices[(i + 1) % 6]);
            }
        } else {
            // Тільки підсвічування
            Handles.color = color;
            Handles.DrawAAConvexPolygon(vertices);
        }

        Handles.EndGUI();
    }
}

// Статичний клас для утиліт роботи з гексагонами
// Ці методи варто перенести у HexUtils
public static class HexGridEditorUtils {
    // Функція для перевірки, чи знаходиться точка всередині гексагона
    public static bool IsPointInHexagon(Vector2 point, Vector2 hexCenter, float radius) {
        // Визначаємо вершини гексагона
        Vector2[] vertices = new Vector2[6];
        for (int i = 0; i < 6; i++) {
            float angle = 2 * Mathf.PI / 6 * i;
            vertices[i] = new Vector2(
                hexCenter.x + radius * Mathf.Cos(angle),
                hexCenter.y + radius * Mathf.Sin(angle)
            );
        }

        // Перевіряємо, чи точка всередині гексагона
        int j = vertices.Length - 1;
        bool inside = false;

        for (int i = 0; i < vertices.Length; j = i++) {
            if (((vertices[i].y > point.y) != (vertices[j].y > point.y)) &&
                (point.x < (vertices[j].x - vertices[i].x) * (point.y - vertices[i].y) /
                (vertices[j].y - vertices[i].y) + vertices[i].x)) {
                inside = !inside;
            }
        }

        return inside;
    }
}
#endif


public static class HexUtils {
    // Шість напрямків для шестикутної сітки (axial coordinates)
    private static readonly Vector2Int[] Directions = {
        new Vector2Int(1, 0), new Vector2Int(1, -1), new Vector2Int(0, -1),
        new Vector2Int(-1, 0), new Vector2Int(-1, 1), new Vector2Int(0, 1)
    };

    public static List<Vector2Int> GetCellsInRadiusOffset(Vector2Int center, int radius, int gridWidth, int gridHeight) {
        List<Vector2Int> results = new List<Vector2Int>();

        for (int dx = -radius; dx <= radius; dx++) {
            for (int dy = Math.Max(-radius, -dx - radius); dy <= Math.Min(radius, -dx + radius); dy++) {
                int dz = -dx - dy;

                // Axial координати
                int q = dx;
                int r = dy;

                // Перетворення axial → offset (even-q)
                Vector2Int axial = new Vector2Int(center.x + q, center.y + r + (center.x % 2 == 0 ? (q - (q & 1)) / 2 : (q + (q & 1)) / 2));

                if (IsValidHexCoordinate(axial.x, axial.y, gridWidth, gridHeight)) {
                    results.Add(axial);
                }
            }
        }

        return results;
    }

    public static List<Vector2Int> GetCellsInRadius(Vector2Int center, int radius, int gridWidth, int gridHeight) {
        List<Vector2Int> cells = new List<Vector2Int>();

        for (int dx = -radius; dx <= radius; dx++) {
            for (int dy = Math.Max(-radius, -dx - radius); dy <= Math.Min(radius, -dx + radius); dy++) {
                int dz = -dx - dy;
                int q = center.x + dx;
                int r = center.y + dy;

                if (IsValidHexCoordinate(q, r, gridWidth, gridHeight)) {
                    cells.Add(new Vector2Int(q, r));
                }
            }
        }

        return cells;
    }


    public static bool IsValidHexCoordinate(int x, int y, int gridWidth, int gridHeight) {
        return x >= 0 && x < gridWidth && y >= 0 && y < gridHeight;
    }

    public static Vector2Int? GetHexCoordinateAtScreenPosition(Vector2 inputPos, Vector2 scrollPosition,
        int gridWidth, int gridHeight, float hexSize = 30f) {
        Vector2 mousePos = new Vector2(inputPos.x, inputPos.y - 215); // Коригування позиції

        // Корегуємо позицію миші враховуючи прокрутку
        Vector2 adjustedPosition = new Vector2(mousePos.x, mousePos.y);
        adjustedPosition.y += scrollPosition.y;
        adjustedPosition.x += scrollPosition.x;

        float hexWidth = hexSize;
        float hexHeight = hexSize;
        float horSpacing = hexWidth * 0.75f;
        float verSpacing = hexHeight * 0.75f;

        // Початкові координати сітки
        float offsetX = 30f;
        float offsetY = 30f;

        for (int yCoord = 0; yCoord < gridHeight; yCoord++) {
            for (int xCoord = 0; xCoord < gridWidth; xCoord++) {
                float rowOffset = xCoord % 2 == 1 ? verSpacing / 2 : 0;
                Vector2 hexCenter = new Vector2(
                    offsetX + xCoord * horSpacing,
                    offsetY + yCoord * verSpacing + rowOffset
                );

                if (IsPointInHex(adjustedPosition, hexCenter, hexWidth / 2)) {
                    return new Vector2Int(xCoord, yCoord);
                }
            }
        }

        return null;
    }

    public static bool IsPointInHex(Vector2 point, Vector2 center, float size) {
        float distance = Vector2.Distance(point, center);
        return distance <= size;
    }

    public static void DrawHexagon(float x, float y, float size, Color color, bool outline) {
        Vector3[] vertices = new Vector3[6];
        for (int i = 0; i < 6; i++) {
            float angle = 2 * Mathf.PI / 6 * i;
            vertices[i] = new Vector2(
                x + size * Mathf.Cos(angle),
                y + size * Mathf.Sin(angle)
            );
        }

        Handles.BeginGUI();

        if (!outline) {
            Handles.color = color;
            Handles.DrawAAConvexPolygon(vertices);
            Handles.color = Color.black;
            for (int i = 0; i < 6; i++) {
                Handles.DrawLine(vertices[i], vertices[(i + 1) % 6]);
            }
        } else {
            Handles.color = color;
            Handles.DrawAAConvexPolygon(vertices);
        }

        Handles.EndGUI();
    }
}