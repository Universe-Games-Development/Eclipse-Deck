using System;
using System.Linq;
using UnityEngine;

#region Data Structures

public struct Grid<T> {
    public readonly GridRow<T>[] Rows;

    public Grid(GridRow<T>[] rows) {
        Rows = rows ?? Array.Empty<GridRow<T>>();
    }

    public Grid(T[] items, int itemsPerRow = -1) {
        if (items == null) throw new ArgumentNullException(nameof(items));

        if (items.Length == 0) {
            Rows = Array.Empty<GridRow<T>>();
            return;
        }

        Rows = itemsPerRow == -1
            ? CreateSingleRow(items)
            : CreateGrid(items, itemsPerRow);
    }

    private static GridRow<T>[] CreateSingleRow(T[] items) {
        return new[] { new GridRow<T>(items) };
    }

    private static GridRow<T>[] CreateGrid(T[] items, int itemsPerRow) {
        int totalItems = items.Length;
        int rowCount = (totalItems + itemsPerRow - 1) / itemsPerRow;
        var rows = new GridRow<T>[rowCount];

        for (int rowIndex = 0; rowIndex < rowCount; rowIndex++) {
            int startIdx = rowIndex * itemsPerRow;
            int cellsInRow = Math.Min(itemsPerRow, totalItems - startIdx);

            T[] cells = new T[cellsInRow];
            Array.Copy(items, startIdx, cells, 0, cellsInRow);

            rows[rowIndex] = new GridRow<T>(cells);
        }

        return rows;
    }

    public int RowCount => Rows.Length;

    public int TotalCells {
        get {
            int total = 0;
            foreach (var row in Rows)
                total += row.Count;
            return total;
        }
    }

    public bool IsEmpty => Rows.Length == 0;
}

[Serializable]
public readonly struct GridRow<T> {
    public readonly T[] Cells;

    public GridRow(T[] cells) {
        Cells = cells ?? Array.Empty<T>();
    }

    public int Count => Cells.Length;
    public bool IsEmpty => Cells.Length == 0;
}

public readonly struct ItemLayoutInfo {
    public readonly string id;
    public readonly Vector3 size;

    public ItemLayoutInfo(string id, Vector3 size) {
        this.id = id;
        this.size = size;
    }
}

[Serializable]
public readonly struct LayoutPoint {
    public readonly Vector3 Position;
    public readonly Quaternion Rotation;
    public readonly string Id; // Замінили Index на Id
    public readonly int Row;
    public readonly int Column;
    public readonly Vector3 Dimensions;

    public LayoutPoint(Vector3 pos, Quaternion rot, string id, int row, int col, Vector3 dims) {
        Position = pos;
        Rotation = rot;
        Id = id;
        Row = row;
        Column = col;
        Dimensions = dims;
    }
}

public readonly struct LayoutMetadata {
    public readonly int TotalItems;
    public readonly int RowsCount;
    public readonly int ItemsPerRow;
    public readonly float TotalWidth;
    public readonly float TotalLength;
    public readonly float CompressionRatio;
    public readonly bool IsCompressed;

    public LayoutMetadata(int total, int rows, int perRow, float width, float length, float compression, bool compressed) {
        TotalItems = total;
        RowsCount = rows;
        ItemsPerRow = perRow;
        TotalWidth = width;
        TotalLength = length;
        CompressionRatio = compression;
        IsCompressed = compressed;
    }
}

public readonly struct LayoutResult {
    public readonly LayoutPoint[] Points;
    public readonly LayoutMetadata Metadata;

    public LayoutResult(LayoutPoint[] points, LayoutMetadata metadata) {
        Points = points;
        Metadata = metadata;
    }
}
#endregion

public class Grid3DLayout : ILayout3DHandler {
    private readonly LayoutSettings _settings;

    public Grid3DLayout(LayoutSettings settings) {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    public LayoutResult Calculate(Grid<ItemLayoutInfo> gridData, bool useDefaultSizes = true) {
        if (gridData.IsEmpty) {
            return new LayoutResult(Array.Empty<LayoutPoint>(), new LayoutMetadata(0, 0, 0, 0, 0, 1, false));
        }

        Grid<ItemLayoutInfo> resultData = gridData;
        if (useDefaultSizes) {
            resultData = AlignDimensionsToDefault(gridData);
        }
        return CalculateLayoutInternal(resultData);
    }

    private Grid<ItemLayoutInfo> AlignDimensionsToDefault(Grid<ItemLayoutInfo> gridData) {
        GridRow<ItemLayoutInfo>[] rows = gridData.Rows;
        GridRow<ItemLayoutInfo>[] newGridRows = new GridRow<ItemLayoutInfo>[gridData.RowCount];

        for (int i = 0; i < rows.Length; i++) {
            ItemLayoutInfo[] cells = rows[i].Cells;
            ItemLayoutInfo[] newCells = new ItemLayoutInfo[cells.Length];

            for (int j = 0; j < cells.Length; j++) {
                ItemLayoutInfo itemLayoutInfo = cells[j];
                // Використовуємо розміри з налаштувань
                ItemLayoutInfo newInfo = new ItemLayoutInfo(itemLayoutInfo.id, _settings.itemSizes);
                newCells[j] = newInfo;
            }
            newGridRows[i] = new GridRow<ItemLayoutInfo>(newCells);
        }

        return new Grid<ItemLayoutInfo>(newGridRows);
    }

    public LayoutResult Calculate(ItemLayoutInfo[] items, bool useDefaultSizes = true) {
        if (items == null || items.Length == 0) {
            return new LayoutResult(Array.Empty<LayoutPoint>(), new LayoutMetadata(0, 0, 0, 0, 0, 1, false));
        }

        // Створюємо сітку з одним рядком
        var gridData = new Grid<ItemLayoutInfo>(items);
        return Calculate(gridData, useDefaultSizes);
    }


    private LayoutResult CalculateLayoutInternal(Grid<ItemLayoutInfo> gridData) {
        int itemsPerRow = DetermineItemsPerRow(gridData);
        int rows = gridData.RowCount;

        var points = new LayoutPoint[gridData.TotalCells];
        float startZ = rows == 1 ? 0f : -(rows - 1) * (_settings.itemSizes.z + _settings.RowSpacing) * 0.5f;

        bool anyCompressed = false;
        int globalIndex = 0;

        for (int rowIndex = 0; rowIndex < rows; rowIndex++) {
            GridRow<ItemLayoutInfo> row = gridData.Rows[rowIndex];
            int itemsInRow = row.Count;

            bool wasCompressed = CalculateRow(row.Cells, rowIndex, rows, startZ, globalIndex, points);
            anyCompressed |= wasCompressed;
            globalIndex += itemsInRow;
        }

        var metadata = CreateMetadata(gridData.TotalCells, itemsPerRow, rows, points, anyCompressed);
        return new LayoutResult(points, metadata);
    }

    private int DetermineItemsPerRow(Grid<ItemLayoutInfo> gridData) {
        if (gridData.IsEmpty) return 0;

        int maxPerRow = 0;
        foreach (var row in gridData.Rows) {
            maxPerRow = Mathf.Max(maxPerRow, row.Count);
        }
        return maxPerRow;
    }

    private bool CalculateRow(ItemLayoutInfo[] infos, int row, int totalRows,
                              float startZ, int startIdx, LayoutPoint[] output) {
        int count = infos.Length;
        var dims = infos.Select(info => info.size).ToArray();

        if (count == 0) return false;

        // Розрахунок ширини елементів та ідеальної ширини ряду
        float totalItemsWidth = SumWidths(dims);
        float idealWidth = totalItemsWidth + (count - 1) * _settings.ColumnSpacing;

        // Визначаємо чи потрібне стискання
        bool needsCompression = idealWidth > _settings.MaxTotalWidth;

        float rowWidth;
        float spacing;
        bool usePositionCompression = false;

        if (!needsCompression) {
            // Режим 1: Все поміщається ідеально
            rowWidth = idealWidth;
            spacing = _settings.ColumnSpacing;
        } else if (_settings.CompressPositionsByTotalWidth) {
            // Режим 2: Агресивне стискання позицій (можливе накладання)
            rowWidth = _settings.MaxTotalWidth;
            spacing = 0f; // Відступів немає, все стискається
            usePositionCompression = true;
        } else {
            // Режим 3: Стискання тільки через зменшення відступів
            float availableSpace = _settings.MaxTotalWidth - totalItemsWidth;
            spacing = count > 1 ? Mathf.Max(0f, availableSpace / (count - 1)) : 0f;
            rowWidth = _settings.MaxTotalWidth;
        }

        // Розрахунок базових координат
        float maxLength = _settings.AlignByLargestInRow ? MaxLength(dims) : _settings.itemSizes.z;
        float zPos = startZ + row * (maxLength + _settings.RowSpacing);

        // Генерація точок
        if (usePositionCompression) {
            CalculateCompressedPositions(infos, row, startIdx, zPos, rowWidth, count, output);
        } else {
            CalculateNormalPositions(infos, row, startIdx, zPos, rowWidth, spacing, count, output);
        }

        return needsCompression;
    }

    private void CalculateNormalPositions(ItemLayoutInfo[] infos, int row, int startIdx,
                                          float zPos, float rowWidth, float spacing, int count,
                                          LayoutPoint[] output) {
        float xPos = -rowWidth * 0.5f;

        for (int col = 0; col < count; col++) {
            var dim = infos[col].size;
            var id = infos[col].id;

            xPos += dim.x * 0.5f;

            var pos = CalculatePosition(xPos, zPos, col, count);
            var rot = CalculateRotation(col, count);

            output[startIdx + col] = new LayoutPoint(pos, rot, id, row, col, dim);

            xPos += dim.x * 0.5f + (col < count - 1 ? spacing : 0f);
        }
    }

    private void CalculateCompressedPositions(ItemLayoutInfo[] infos, int row, int startIdx,
                                              float zPos, float maxWidth, int count,
                                              LayoutPoint[] output) {
        // Рівномірно розподіляємо елементи по доступній ширині
        float startX = -maxWidth * 0.5f;
        float endX = maxWidth * 0.5f;

        for (int col = 0; col < count; col++) {
            var dim = infos[col].size;
            var id = infos[col].id;

            // Лінійна інтерполяція позицій від початку до кінця
            float t = count > 1 ? (float)col / (count - 1) : 0.5f;
            float xPos = Mathf.Lerp(startX, endX, t);

            var pos = CalculatePosition(xPos, zPos, col, count);
            var rot = CalculateRotation(col, count);

            output[startIdx + col] = new LayoutPoint(pos, rot, id, row, col, dim);
        }
    }

    private Vector3 CalculatePosition(float x, float z, int col, int total) {
        if (total == 1) return new Vector3(x, 0f, z);

        float t = (float)col / (total - 1);
        return new Vector3(x, -t * _settings.DepthOffset, z - t * _settings.VerticalOffset);
    }

    private Quaternion CalculateRotation(int col, int total) {
        if (total == 1) return Quaternion.identity;

        float t = (float)col / (total - 1);
        float angle = Mathf.Lerp(-_settings.MaxRotationAngle, _settings.MaxRotationAngle, t);
        return Quaternion.Euler(0f, angle, 0f);
    }

    private LayoutMetadata CreateMetadata(int total, int perRow, int rows, LayoutPoint[] points, bool compressed) {
        float width = CalculateTotalWidth(points);
        float length = rows == 1 ? _settings.itemSizes.z
            : (rows - 1) * (_settings.itemSizes.z + _settings.RowSpacing);
        float compression = CalculateCompression(points);

        return new LayoutMetadata(total, rows, perRow, width, length, compression, compressed);
    }

    private float CalculateTotalWidth(LayoutPoint[] points) {
        if (points.Length == 0) return 0f;

        float min = float.MaxValue, max = float.MinValue;
        foreach (var p in points) {
            float half = p.Dimensions.x * 0.5f;
            min = Mathf.Min(min, p.Position.x - half);
            max = Mathf.Max(max, p.Position.x + half);
        }
        return max - min;
    }

    private float CalculateCompression(LayoutPoint[] points) {
        if (points.Length == 0) return 1f;

        float totalWidth = 0f;
        int count = 0;

        foreach (var p in points) {
            if (p.Row == 0) {
                totalWidth += p.Dimensions.x;
                count++;
            }
        }

        if (count == 0) return 1f;

        float idealWidth = totalWidth + (count - 1) * _settings.ColumnSpacing;
        float actualWidth = 0f;

        float min = float.MaxValue, max = float.MinValue;
        foreach (var p in points) {
            if (p.Row == 0) {
                float half = p.Dimensions.x * 0.5f;
                min = Mathf.Min(min, p.Position.x - half);
                max = Mathf.Max(max, p.Position.x + half);
            }
        }
        actualWidth = max - min;

        return actualWidth / idealWidth;
    }

    // Утиліти
    private static float SumWidths(Vector3[] sizes) {
        float sum = 0f;
        for (int i = 0; i < sizes.Length; i++)
            sum += sizes[i].x;
        return sum;
    }

    private static float MaxLength(Vector3[] sizes) {
        float max = 0f;
        for (int i = 0; i < sizes.Length; i++)
            max = Mathf.Max(max, sizes[i].z);
        return max;
    }
}
