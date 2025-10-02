using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class LayoutPoint {
    public Vector3 position;
    public Quaternion rotation;
    public int orderIndex;
    public int rowIndex;
    public int columnIndex;

    public LayoutPoint(Vector3 pos, Quaternion rot, int order, int row = 0, int col = 0) {
        position = pos;
        rotation = rot;
        orderIndex = order;
        rowIndex = row;
        columnIndex = col;
    }
}

public class RowLayoutSettings {
    public int ItemsPerRow;
    public int RowsCount;

    public RowLayoutSettings(int itemsPerRow, int rowsCount) {
        ItemsPerRow = itemsPerRow;
        RowsCount = rowsCount;
    }
}

// Результат розрахунку макету з метаданими
public class LayoutResult {
    public List<LayoutPoint> Points { get; set; }
    public LayoutMetadata Metadata { get; set; }

    public LayoutResult(List<LayoutPoint> points, LayoutMetadata metadata) {
        Points = points;
        Metadata = metadata;
    }
}

// Метадані макету для подальшого використання
public class LayoutMetadata {
    public int TotalItems { get; set; }
    public int RowsCount { get; set; }
    public int ItemsPerRow { get; set; }
    public float TotalWidth { get; set; }
    public float TotalLength { get; set; }
    public List<RowMetadata> Rows { get; set; }
    public float CompressionRatio { get; set; }
    public bool HasOverlapping { get; set; }
    public float MaxOverlapAmount { get; set; }

    public LayoutMetadata() {
        Rows = new List<RowMetadata>();
    }
}

// Метадані окремого ряду
public class RowMetadata {
    public int RowIndex { get; set; }
    public int ItemCount { get; set; }
    public float StartX { get; set; }
    public float Spacing { get; set; }
    public float RowWidth { get; set; }
    public float ZPosition { get; set; }
    public bool IsOverlapping { get; set; }
    public float OverlapAmount { get; set; }
    public float CompressionRatio { get; set; }
}

public interface ILayout3DHandler {
    LayoutResult CalculateLayout(int itemsCount, RowLayoutSettings rowSettings = null);
}

public class Linear3DLayout : ILayout3DHandler {
    private readonly LayoutSettings _settings;

    public Linear3DLayout(LayoutSettings settings) {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    public LayoutResult CalculateLayout(int itemsCount, RowLayoutSettings rowSettings = null) {
        if (itemsCount <= 0)
            return new LayoutResult(new List<LayoutPoint>(), new LayoutMetadata { TotalItems = 0 });

        // Single point always in center
        if (itemsCount == 1) {
            var metadata = new LayoutMetadata {
                TotalItems = 1,
                RowsCount = 1,
                ItemsPerRow = 1,
                TotalWidth = _settings.ItemWidth + 2 * _settings.ColumnSpacing,
                TotalLength = _settings.ItemLength,
                CompressionRatio = 1f,
                HasOverlapping = false,
                MaxOverlapAmount = 0f
            };

            metadata.Rows.Add(new RowMetadata {
                RowIndex = 0,
                ItemCount = 1,
                StartX = 0f,
                Spacing = 0f,
                RowWidth = 0f,
                ZPosition = 0f,
                IsOverlapping = false,
                OverlapAmount = 0f,
                CompressionRatio = 1f
            });

            var points = new List<LayoutPoint> {
                new LayoutPoint(Vector3.zero, Quaternion.identity, 0, 0, 0)
            };

            return new LayoutResult(points, metadata);
        }

        // Determine layout parameters
        int itemsPerRow, rowsCount;
        if (rowSettings == null || rowSettings.RowsCount == 1 || rowSettings.ItemsPerRow >= itemsCount) {
            itemsPerRow = itemsCount;
            rowsCount = 1;
        } else {
            itemsPerRow = rowSettings.ItemsPerRow;
            rowsCount = Mathf.CeilToInt((float)itemsCount / itemsPerRow);
        }

        return CalculateUniversalLayout(itemsCount, itemsPerRow, rowsCount);
    }

    private LayoutResult CalculateUniversalLayout(int totalItems, int itemsPerRow, int rowsCount) {
        var points = new List<LayoutPoint>();
        var metadata = new LayoutMetadata {
            TotalItems = totalItems,
            RowsCount = rowsCount,
            ItemsPerRow = itemsPerRow
        };

        // Calculate total height for centering - FIXED
        float totalLength;
        float startZ;

        if (rowsCount == 1) {
            // Single row - no offset needed, place at center
            totalLength = _settings.ItemLength;
            startZ = 0f;
        } else {
            // Multiple rows - center them
            totalLength = (rowsCount - 1) * (_settings.ItemLength + _settings.RowSpacing);
            startZ = -totalLength / 2f;
        }

        metadata.TotalLength = totalLength;

        int itemIndex = 0;
        float maxCompressionRatio = 1f;
        bool hasOverlapping = false;
        float maxOverlapAmount = 0f;
        float maxRowWidth = 0f;

        for (int row = 0; row < rowsCount; row++) {
            int itemsInThisRow = Mathf.Min(itemsPerRow, totalItems - itemIndex);
            if (itemsInThisRow <= 0) break;

            // FIXED: For single row, zPos remains 0, for multiple rows it's calculated from startZ
            float zPos = rowsCount == 1 ? 0f : startZ + row * (_settings.ItemLength + _settings.RowSpacing);

            // Calculate row layout data with metadata
            var rowResult = CalculateRowLayoutData(itemsInThisRow);
            var rowMetadata = rowResult.Metadata;
            rowMetadata.RowIndex = row;
            rowMetadata.ItemCount = itemsInThisRow;
            rowMetadata.ZPosition = zPos;

            metadata.Rows.Add(rowMetadata);

            // Track overall statistics
            maxCompressionRatio = Mathf.Min(maxCompressionRatio, rowMetadata.CompressionRatio);
            if (rowMetadata.IsOverlapping) {
                hasOverlapping = true;
                maxOverlapAmount = Mathf.Max(maxOverlapAmount, rowMetadata.OverlapAmount);
            }
            maxRowWidth = Mathf.Max(maxRowWidth, rowMetadata.RowWidth);

            for (int col = 0; col < itemsInThisRow; col++) {
                Vector3 position = CalculateItemPosition(col, itemsInThisRow, rowResult, zPos, row, rowsCount);
                Quaternion rotation = CalculateItemRotation(col, itemsInThisRow, row);

                points.Add(new LayoutPoint(position, rotation, itemIndex, row, col));
                itemIndex++;
            }
        }

        // Set final metadata
        metadata.TotalWidth = maxRowWidth;
        metadata.CompressionRatio = maxCompressionRatio;
        metadata.HasOverlapping = hasOverlapping;
        metadata.MaxOverlapAmount = maxOverlapAmount;

        return new LayoutResult(points, metadata);
    }

    private RowLayoutResult CalculateRowLayoutData(int itemsInRow) {
        var metadata = new RowMetadata();

        if (itemsInRow <= 0) {
            return new RowLayoutResult(new RowLayoutData(), metadata);
        }

        if (itemsInRow == 1) {
            metadata.StartX = 0f;
            metadata.Spacing = 0f;
            metadata.RowWidth = _settings.ItemWidth;
            metadata.CompressionRatio = 1f;
            metadata.IsOverlapping = false;
            metadata.OverlapAmount = 0f;

            return new RowLayoutResult(
                new RowLayoutData { StartX = 0f, Spacing = 0f },
                metadata
            );
        }

        float minSpacing = _settings.ItemWidth * 0.05f;
        float idealSpacing = _settings.ColumnSpacing;
        float idealTotalWidth = (itemsInRow * _settings.ItemWidth) + ((itemsInRow - 1) * idealSpacing);

        // Ideal case
        if (idealTotalWidth <= _settings.MaxTotalWidth) {
            float startX_2 = -idealTotalWidth / 2f + _settings.ItemWidth / 2f;

            metadata.StartX = startX_2;
            metadata.Spacing = _settings.ItemWidth + idealSpacing;
            metadata.RowWidth = idealTotalWidth;
            metadata.CompressionRatio = 1f;
            metadata.IsOverlapping = false;
            metadata.OverlapAmount = 0f;

            return new RowLayoutResult(
                new RowLayoutData { StartX = startX_2, Spacing = _settings.ItemWidth + idealSpacing },
                metadata
            );
        }

        float minTotalWidth = (itemsInRow * _settings.ItemWidth) + ((itemsInRow - 1) * minSpacing);

        // Overlapping case
        if (minTotalWidth > _settings.MaxTotalWidth) {
            float overlapWidth = _settings.MaxTotalWidth;
            float overlapSpacing = (overlapWidth - itemsInRow * _settings.ItemWidth) / (itemsInRow - 1);
            float startX_2 = -overlapWidth / 2f + _settings.ItemWidth / 2f;
            float overlapAmount = Mathf.Abs(overlapSpacing) / _settings.ItemWidth;

            metadata.StartX = startX_2;
            metadata.Spacing = _settings.ItemWidth + overlapSpacing;
            metadata.RowWidth = overlapWidth;
            metadata.CompressionRatio = overlapWidth / idealTotalWidth;
            metadata.IsOverlapping = true;
            metadata.OverlapAmount = overlapAmount;

            return new RowLayoutResult(
                new RowLayoutData { StartX = startX_2, Spacing = _settings.ItemWidth + overlapSpacing },
                metadata
            );
        }

        // Compressed spacing case
        float availableWidth = _settings.MaxTotalWidth;
        float compressedSpacing = (availableWidth - itemsInRow * _settings.ItemWidth) / (itemsInRow - 1);
        compressedSpacing = Mathf.Max(compressedSpacing, minSpacing);

        float actualTotalWidth = (itemsInRow * _settings.ItemWidth) + ((itemsInRow - 1) * compressedSpacing);
        float startX = -actualTotalWidth / 2f + _settings.ItemWidth / 2f;

        metadata.StartX = startX;
        metadata.Spacing = _settings.ItemWidth + compressedSpacing;
        metadata.RowWidth = actualTotalWidth;
        metadata.CompressionRatio = actualTotalWidth / idealTotalWidth;
        metadata.IsOverlapping = false;
        metadata.OverlapAmount = 0f;

        return new RowLayoutResult(
            new RowLayoutData { StartX = startX, Spacing = _settings.ItemWidth + compressedSpacing },
            metadata
        );
    }

    private Vector3 CalculateItemPosition(int index, int itemsInRow, RowLayoutResult rowResult, float zOffset, int rowIndex, int totalRows) {
        var layoutData = rowResult.LayoutData;
        var metadata = rowResult.Metadata;

        float xPos = metadata.StartX + index * metadata.Spacing;
        float yPos = 0f;
        float zPos = zOffset;

        // Apply 3D effects
        if (itemsInRow > 1) {
            float normalizedIndex = (float)index / (itemsInRow - 1);
            float rowEffect = totalRows > 1 ? (1f - (float)rowIndex / (totalRows - 1)) * 0.5f + 0.5f : 1f;

            yPos = -normalizedIndex * _settings.DepthOffset * rowEffect;
            zPos -= normalizedIndex * _settings.VerticalOffset * rowEffect;
        }

        // Position variation
        if (_settings.PositionVariation > 0f) {
            float seed = (index + rowIndex * 100) * 12.9898f;
            float randomX = (Mathf.Sin(seed) * 2f - 1f) * _settings.PositionVariation;
            float randomY = (Mathf.Sin(seed * 1.5f) * 2f - 1f) * _settings.PositionVariation * 0.1f;

            xPos += randomX;
            yPos += randomY;
        }

        return new Vector3(xPos, yPos, zPos);
    }

    private Quaternion CalculateItemRotation(int index, int itemsInRow, int rowIndex) {
        if (itemsInRow <= 1) return Quaternion.identity;

        float t = (float)index / (itemsInRow - 1);
        float angle = Mathf.Lerp(-_settings.MaxRotationAngle, _settings.MaxRotationAngle, t);

        if (_settings.RotationOffset > 0f) {
            float seed = (index + rowIndex * 100) * 23.1406f;
            float randomOffset = (Mathf.Sin(seed) * 2f - 1f) * _settings.RotationOffset;
            angle += randomOffset;
        }

        return Quaternion.Euler(0f, angle, 0f);
    }

    // Helper classes for internal calculations
    private class RowLayoutData {
        public float StartX;
        public float Spacing;
    }

    private class RowLayoutResult {
        public RowLayoutData LayoutData { get; }
        public RowMetadata Metadata { get; }

        public RowLayoutResult(RowLayoutData layoutData, RowMetadata metadata) {
            LayoutData = layoutData;
            Metadata = metadata;
        }
    }
}