using System;
using System.Collections.Generic;
using UnityEngine;

#region Data Structures

public struct Grid<T> {
    public readonly GridRow<T>[] Rows;
    public readonly int RowCount;
    public readonly int TotalCells;

    public Grid(GridRow<T>[] rows) {
        Rows = rows ?? Array.Empty<GridRow<T>>();
        RowCount = Rows.Length;
        TotalCells = CalculateTotalCells(Rows);
    }

    public Grid(T[] items, int itemsPerRow = -1) {
        if (items == null) throw new ArgumentNullException(nameof(items));

        if (items.Length == 0) {
            Rows = Array.Empty<GridRow<T>>();
            RowCount = 0;
            TotalCells = 0;
            return;
        }

        Rows = itemsPerRow == -1
            ? CreateSingleRow(items)
            : CreateGrid(items, itemsPerRow);

        RowCount = Rows.Length;
        TotalCells = CalculateTotalCells(Rows);
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

    private static int CalculateTotalCells(GridRow<T>[] rows) {
        int total = 0;
        foreach (var row in rows)
            total += row.Count;
        return total;
    }

    public bool IsEmpty => RowCount == 0;
}

[Serializable]
public readonly struct GridRow<T> {
    public readonly T[] Cells;
    public readonly int Count;
    public readonly bool IsEmpty;

    public GridRow(T[] cells) {
        Cells = cells ?? Array.Empty<T>();
        Count = Cells.Length;
        IsEmpty = Count == 0;
    }
}

public readonly struct ItemLayoutInfo {
    public readonly string Id;
    public readonly Vector3 VisualSize;

    public ItemLayoutInfo(string id, Vector3 visualSize) {
        Id = id;
        VisualSize = visualSize;
    }
}

[Serializable]
public readonly struct LayoutPoint {
    public readonly Vector3 Position;
    public readonly Quaternion Rotation;
    public readonly string Id;
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

    // Helper для створення копії з новою позицією
    public LayoutPoint WithPosition(Vector3 newPosition) {
        return new LayoutPoint(newPosition, Rotation, Id, Row, Column, Dimensions);
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

    public static LayoutResult Empty => new LayoutResult(Array.Empty<LayoutPoint>(), new LayoutMetadata(0, 0, 0, 0f, 0f, 1f, false));
}

#endregion

#region Grid Alignment Modes

/// <summary>
/// Режим вирівнювання елементів у сітці
/// </summary>
public enum GridAlignmentMode {
    /// <summary>
    /// Кожен ряд центрується незалежно (за замовчуванням)
    /// Елементи можуть не співпадати по колонках
    /// </summary>
    IndependentRows,

    /// <summary>
    /// Всі колонки вирівнюються по найширшому елементу
    /// Елементи чітко відповідають своїм колонкам
    /// </summary>
    AlignedColumns
}

#endregion

#region Column Width Calculator

/// <summary>
/// Обчислює максимальні ширини колонок для вирівнювання
/// </summary>
public static class ColumnWidthCalculator {
    /// <summary>
    /// Обчислює максимальну ширину для кожної колонки
    /// </summary>
    public static float[] CalculateColumnWidths(Grid<ItemLayoutInfo> gridData) {
        if (gridData.IsEmpty) return Array.Empty<float>();

        // Знаходимо максимальну кількість колонок
        int maxColumns = 0;
        foreach (var row in gridData.Rows) {
            maxColumns = Mathf.Max(maxColumns, row.Count);
        }

        var columnWidths = new float[maxColumns];

        // Для кожної колонки знаходимо максимальну ширину
        for (int col = 0; col < maxColumns; col++) {
            float maxWidth = 0f;

            foreach (var row in gridData.Rows) {
                if (col < row.Count) {
                    maxWidth = Mathf.Max(maxWidth, row.Cells[col].VisualSize.x);
                }
            }

            columnWidths[col] = maxWidth;
        }

        return columnWidths;
    }

    /// <summary>
    /// Перераховує позиції елементів з врахуванням ширин колонок
    /// </summary>
    public static LayoutPoint[] RealignToColumns(
        LayoutPoint[] points,
        float[] columnWidths,
        float spacing,
        int maxColumnsPerRow) {

        if (points.Length == 0 || columnWidths.Length == 0) return points;

        var realignedPoints = new LayoutPoint[points.Length];

        // Обчислюємо загальну ширину сітки
        float totalWidth = 0f;
        for (int i = 0; i < columnWidths.Length; i++) {
            totalWidth += columnWidths[i];
            if (i < columnWidths.Length - 1) {
                totalWidth += spacing;
            }
        }

        // Перераховуємо позиції для кожного елемента
        for (int i = 0; i < points.Length; i++) {
            var point = points[i];
            int col = point.Column;

            // Обчислюємо X позицію на основі колонок
            float xPos = CalculateColumnXPosition(col, columnWidths, spacing, totalWidth);

            // Створюємо новий point з оновленою позицією
            var newPosition = new Vector3(xPos, point.Position.y, point.Position.z);
            realignedPoints[i] = point.WithPosition(newPosition);
        }

        return realignedPoints;
    }

    private static float CalculateColumnXPosition(
        int columnIndex,
        float[] columnWidths,
        float spacing,
        float totalWidth) {

        // Стартова позиція (ліва границя)
        float startX = -totalWidth * 0.5f;

        // Додаємо ширини попередніх колонок і spacing
        float xPos = startX;
        for (int i = 0; i < columnIndex; i++) {
            xPos += columnWidths[i] + spacing;
        }

        // Додаємо половину ширини поточної колонки (центруємо в колонці)
        xPos += columnWidths[columnIndex] * 0.5f;

        return xPos;
    }
}

#endregion

#region Updated Grid3DLayout with Alignment Modes

public class Grid3DLayout : IGridLayout {
    private readonly GridLayoutSettings _settings;
    private readonly Linear3DLayout _horizontalLayout;
    private readonly Linear3DLayout _verticalLayout;

    public Grid3DLayout(GridLayoutSettings settings) {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        ValidateSettings(settings);

        _horizontalLayout = new Linear3DLayout(settings.horizontalSettings);
        _verticalLayout = new Linear3DLayout(settings.verticalSettings);
    }

    private void ValidateSettings(GridLayoutSettings settings) {
        if (settings.horizontalSettings == null) {
            throw new ArgumentException("GridLayoutSettings.horizontalSettings cannot be null");
        }
        if (settings.verticalSettings == null) {
            throw new ArgumentException("GridLayoutSettings.verticalSettings cannot be null");
        }
    }

    public LayoutResult Calculate(ItemLayoutInfo[] items) {
        if (items == null) throw new ArgumentNullException(nameof(items));
        if (items.Length == 0) return LayoutResult.Empty;

        var gridData = new Grid<ItemLayoutInfo>(items, items.Length);
        return Calculate(gridData);
    }

    public LayoutResult Calculate(Grid<ItemLayoutInfo> gridData) {
        if (gridData.IsEmpty) return LayoutResult.Empty;
        return CalculateDualLinearLayout(gridData);
    }

    private LayoutResult CalculateDualLinearLayout(Grid<ItemLayoutInfo> gridData) {
        if (gridData.RowCount == 0) return LayoutResult.Empty;

        // Крок 1: Розрахувати горизонтальне розташування для кожного ряду
        var rowLayouts = CalculateRowLayouts(gridData);

        // Крок 2: Розрахувати вертикальне розташування рядів
        var verticalLayoutResult = CalculateRowPositions(rowLayouts);

        // Крок 3: Об'єднати результати з правильним маппінгом осей
        var allPoints = CombineLayoutsWithAxisMapping(gridData, rowLayouts, verticalLayoutResult);

        // Крок 4: Вирівнювання колонок (якщо потрібно)
        if (_settings.alignmentMode == GridAlignmentMode.AlignedColumns) {
            allPoints = AlignColumnsIfNeeded(gridData, allPoints);
        }

        // Крок 5: Створити метадані
        var metadata = CreateMetadata(gridData, rowLayouts, allPoints);

        return new LayoutResult(allPoints, metadata);
    }

    private LayoutResult[] CalculateRowLayouts(Grid<ItemLayoutInfo> gridData) {
        var rowLayouts = new LayoutResult[gridData.RowCount];

        for (int rowIndex = 0; rowIndex < gridData.RowCount; rowIndex++) {
            var row = gridData.Rows[rowIndex];
            rowLayouts[rowIndex] = _horizontalLayout.Calculate(row.Cells);
        }

        return rowLayouts;
    }

    private LayoutResult CalculateRowPositions(LayoutResult[] rowLayouts) {
        int rowCount = rowLayouts.Length;
        var rowHeights = new float[rowCount];

        for (int i = 0; i < rowCount; i++) {
            rowHeights[i] = GetRowHeight(rowLayouts[i]);
        }

        var rowItems = new ItemLayoutInfo[rowCount];
        for (int i = 0; i < rowCount; i++) {
            rowItems[i] = new ItemLayoutInfo(
                $"Row_{i}",
                new Vector3(rowHeights[i], 0f, 0f)
            );
        }

        return _verticalLayout.Calculate(rowItems);
    }

    private float GetRowHeight(LayoutResult rowLayout) {
        float maxHeight = 0f;
        foreach (var point in rowLayout.Points) {
            maxHeight = Mathf.Max(maxHeight, point.Dimensions.z);
        }
        return maxHeight > 0f ? maxHeight : 1f;
    }

    private LayoutPoint[] CombineLayoutsWithAxisMapping(
        Grid<ItemLayoutInfo> gridData,
        LayoutResult[] rowLayouts,
        LayoutResult verticalLayout) {

        var allPoints = new LayoutPoint[gridData.TotalCells];
        int pointIndex = 0;

        var verticalPoints = verticalLayout.Points;

        for (int rowIndex = 0; rowIndex < gridData.RowCount; rowIndex++) {
            var rowPoints = rowLayouts[rowIndex].Points;
            var verticalPoint = verticalPoints[rowIndex];

            Vector3 mappedVerticalPos = new Vector3(
                0f,
                verticalPoint.Position.y,
                verticalPoint.Position.x
            );

            Quaternion mappedVerticalRot = verticalPoint.Rotation;

            foreach (var point in rowPoints) {
                Vector3 finalPosition = new Vector3(
                    point.Position.x,
                    point.Position.y + mappedVerticalPos.y,
                    mappedVerticalPos.z + point.Position.z
                );

                Quaternion finalRotation = mappedVerticalRot * point.Rotation;

                allPoints[pointIndex] = new LayoutPoint(
                    finalPosition,
                    finalRotation,
                    point.Id,
                    rowIndex,
                    point.Column,
                    point.Dimensions
                );

                pointIndex++;
            }
        }

        return allPoints;
    }

    private LayoutPoint[] AlignColumnsIfNeeded(Grid<ItemLayoutInfo> gridData, LayoutPoint[] points) {
        // Обчислюємо ширини колонок
        var columnWidths = ColumnWidthCalculator.CalculateColumnWidths(gridData);

        // Знаходимо максимальну кількість колонок
        int maxColumns = 0;
        foreach (var row in gridData.Rows) {
            maxColumns = Mathf.Max(maxColumns, row.Count);
        }

        // Перераховуємо позиції
        return ColumnWidthCalculator.RealignToColumns(
            points,
            columnWidths,
            _settings.horizontalSettings.ItemSpacing,
            maxColumns
        );
    }

    private LayoutMetadata CreateMetadata(
        Grid<ItemLayoutInfo> gridData,
        LayoutResult[] rowLayouts,
        LayoutPoint[] allPoints) {

        float maxWidth = 0f;
        bool anyCompressed = false;
        float totalCompressionRatio = 0f;

        foreach (var rowLayout in rowLayouts) {
            maxWidth = Mathf.Max(maxWidth, rowLayout.Metadata.TotalWidth);
            anyCompressed |= rowLayout.Metadata.IsCompressed;
            totalCompressionRatio += rowLayout.Metadata.CompressionRatio;
        }

        float avgCompressionRatio = rowLayouts.Length > 0
            ? totalCompressionRatio / rowLayouts.Length
            : 1f;

        float totalLength = CalculateTotalDepth(allPoints);

        return new LayoutMetadata(
            gridData.TotalCells,
            gridData.RowCount,
            GetMaxItemsPerRow(gridData),
            maxWidth,
            totalLength,
            avgCompressionRatio,
            anyCompressed
        );
    }

    private float CalculateTotalDepth(LayoutPoint[] points) {
        if (points.Length == 0) return 0f;

        float minZ = float.MaxValue;
        float maxZ = float.MinValue;

        foreach (var p in points) {
            float halfDepth = p.Dimensions.z * 0.5f;
            minZ = Mathf.Min(minZ, p.Position.z - halfDepth);
            maxZ = Mathf.Max(maxZ, p.Position.z + halfDepth);
        }

        return maxZ - minZ;
    }

    private int GetMaxItemsPerRow(Grid<ItemLayoutInfo> gridData) {
        int max = 0;
        foreach (var row in gridData.Rows) {
            max = Mathf.Max(max, row.Count);
        }
        return max;
    }
}

#endregion

#region Linear3DLayout

public class Linear3DLayout : ILinearLayout {
    private readonly LinearLayoutSettings _settings;
    private readonly ICompressionStrategy _compressionStrategy;

    public Linear3DLayout(LinearLayoutSettings settings) {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _compressionStrategy = CompressionStrategyFactory.Create(settings.compressionMode);
    }

    public LayoutResult Calculate(ItemLayoutInfo[] items) {
        if (items == null || items.Length == 0) return LayoutResult.Empty;
        return CalculateLinearLayout(items, 0, 0f);
    }

    public LayoutResult CalculateLinearLayout(ItemLayoutInfo[] items, int rowIndex, float zPosition) {
        if (items == null || items.Length == 0) return LayoutResult.Empty;

        int count = items.Length;
        var sizes = ExtractSizes(items);
        var compression = CalculateCompression(sizes, count);

        var points = compression.UsePositionInterpolation
            ? GenerateInterpolatedPositions(items, sizes, zPosition, compression.ActualWidth, rowIndex)
            : GenerateSpacedPositions(items, sizes, zPosition, compression.Spacing, compression.ActualWidth, rowIndex);

        var metadata = CreateMetadata(count, compression, sizes);
        return new LayoutResult(points, metadata);
    }

    private Vector3[] ExtractSizes(ItemLayoutInfo[] items) {
        var sizes = new Vector3[items.Length];
        for (int i = 0; i < items.Length; i++) {
            sizes[i] = items[i].VisualSize;
        }
        return sizes;
    }

    private CompressionResult CalculateCompression(Vector3[] sizes, int count) {
        float totalWidth = SumWidths(sizes);
        return _compressionStrategy.Calculate(
            totalWidth,
            _settings.ItemSpacing,
            count,
            _settings.MaxTotalWidth
        );
    }

    private LayoutPoint[] GenerateSpacedPositions(
        ItemLayoutInfo[] items,
        Vector3[] sizes,
        float zPos,
        float spacing,
        float rowWidth,
        int rowIndex) {

        int count = items.Length;
        var points = new LayoutPoint[count];
        float xPos = -rowWidth * 0.5f;

        for (int i = 0; i < count; i++) {
            xPos += sizes[i].x * 0.5f;
            points[i] = CreateLayoutPoint(items[i], sizes[i], xPos, zPos, i, count, rowIndex);
            xPos += sizes[i].x * 0.5f + (i < count - 1 ? spacing : 0f);
        }

        return points;
    }

    private LayoutPoint[] GenerateInterpolatedPositions(
        ItemLayoutInfo[] items,
        Vector3[] sizes,
        float zPos,
        float maxWidth,
        int rowIndex) {

        int count = items.Length;
        var points = new LayoutPoint[count];
        float startX = -maxWidth * 0.5f;
        float endX = maxWidth * 0.5f;

        for (int i = 0; i < count; i++) {
            float t = count > 1 ? (float)i / (count - 1) : 0.5f;
            float xPos = Mathf.Lerp(startX, endX, t);
            points[i] = CreateLayoutPoint(items[i], sizes[i], xPos, zPos, i, count, rowIndex);
        }

        return points;
    }

    private LayoutPoint CreateLayoutPoint(
        ItemLayoutInfo item,
        Vector3 size,
        float x,
        float z,
        int index,
        int total,
        int row) {

        var position = CalculatePosition(x, z, index, total);
        var rotation = CalculateRotation(index, total);

        return new LayoutPoint(position, rotation, item.Id, row, index, size);
    }

    private Vector3 CalculatePosition(float x, float z, int index, int total) {
        if (total == 1) return new Vector3(x, 0f, z);

        float t = (float)index / (total - 1);
        float y = -t * _settings.DepthOffset;
        float zOffset = z - t * _settings.VerticalOffset;

        return new Vector3(x, y, zOffset);
    }

    private Quaternion CalculateRotation(int index, int total) {
        if (total == 1 || _settings.MaxRotationAngle == 0f) {
            return Quaternion.identity;
        }

        float t = (float)index / (total - 1);
        float angle = Mathf.Lerp(-_settings.MaxRotationAngle, _settings.MaxRotationAngle, t);
        return Quaternion.Euler(0f, angle, 0f);
    }

    private LayoutMetadata CreateMetadata(int count, CompressionResult compression, Vector3[] sizes) {
        float totalWidth = SumWidths(sizes);
        float maxLength = MaxLength(sizes);
        float idealWidth = totalWidth + (count - 1) * _settings.ItemSpacing;
        float compressionRatio = idealWidth > 0f ? compression.ActualWidth / idealWidth : 1f;

        return new LayoutMetadata(
            count,
            1,
            count,
            compression.ActualWidth,
            maxLength,
            compressionRatio,
            compression.IsCompressed
        );
    }

    private static float SumWidths(Vector3[] sizes) {
        float sum = 0f;
        foreach (var size in sizes) sum += size.x;
        return sum;
    }

    private static float MaxLength(Vector3[] sizes) {
        float max = 0f;
        foreach (var size in sizes) max = Mathf.Max(max, size.z);
        return max;
    }
}

#endregion

#region Compression Strategies

public interface ICompressionStrategy {
    CompressionResult Calculate(float totalItemsWidth, float idealSpacing, int itemCount, float maxWidth);
}

public readonly struct CompressionResult {
    public readonly float Spacing;
    public readonly float ActualWidth;
    public readonly bool IsCompressed;
    public readonly bool UsePositionInterpolation;

    public CompressionResult(float spacing, float actualWidth, bool compressed, bool interpolate) {
        Spacing = spacing;
        ActualWidth = actualWidth;
        IsCompressed = compressed;
        UsePositionInterpolation = interpolate;
    }
}

public class NoCompressionStrategy : ICompressionStrategy {
    public CompressionResult Calculate(float totalItemsWidth, float idealSpacing, int itemCount, float maxWidth) {
        float idealWidth = totalItemsWidth + (itemCount - 1) * idealSpacing;
        return new CompressionResult(idealSpacing, idealWidth, false, false);
    }
}

public class ReduceSpacingStrategy : ICompressionStrategy {
    public CompressionResult Calculate(float totalItemsWidth, float idealSpacing, int itemCount, float maxWidth) {
        float idealWidth = totalItemsWidth + (itemCount - 1) * idealSpacing;

        if (idealWidth <= maxWidth) {
            return new CompressionResult(idealSpacing, idealWidth, false, false);
        }

        float availableSpace = maxWidth - totalItemsWidth;
        float spacing = itemCount > 1 ? Mathf.Max(0f, availableSpace / (itemCount - 1)) : 0f;

        return new CompressionResult(spacing, maxWidth, true, false);
    }
}

public class CompressPositionsStrategy : ICompressionStrategy {
    public CompressionResult Calculate(float totalItemsWidth, float idealSpacing, int itemCount, float maxWidth) {
        float idealWidth = totalItemsWidth + (itemCount - 1) * idealSpacing;

        if (idealWidth <= maxWidth) {
            return new CompressionResult(idealSpacing, idealWidth, false, false);
        }

        return new CompressionResult(0f, maxWidth, true, true);
    }
}

public static class CompressionStrategyFactory {
    private static readonly Dictionary<CompressionMode, ICompressionStrategy> _strategies = new Dictionary<CompressionMode, ICompressionStrategy> {
        [CompressionMode.None] = new NoCompressionStrategy(),
        [CompressionMode.ReduceSpacing] = new ReduceSpacingStrategy(),
        [CompressionMode.CompressPositions] = new CompressPositionsStrategy()
    };

    public static ICompressionStrategy Create(CompressionMode mode) {
        return _strategies.TryGetValue(mode, out var strategy) ? strategy : _strategies[CompressionMode.ReduceSpacing];
    }
}

#endregion