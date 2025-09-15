using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public class LayoutPoint {
    public Vector3 position;
    public Quaternion rotation;
    public int orderIndex;

    public LayoutPoint(Vector3 pos, Quaternion rot, int order) {
        position = pos;
        rotation = rot;
        orderIndex = order;
    }
}

public interface ILayout3DHandler {
    List<LayoutPoint> CalculateCardTransforms(int cardCount);
}

public class Linear3DLayout : ILayout3DHandler {
    private readonly Linear3DHandLayoutSettings _settings;

    public Linear3DLayout(Linear3DHandLayoutSettings settings) {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    public List<LayoutPoint> CalculateCardTransforms(int cardCount) {
        var transforms = new List<LayoutPoint>();
        if (cardCount <= 0) return transforms;

        // Одна карта завжди в центрі
        if (cardCount == 1) {
            transforms.Add(new LayoutPoint(
                Vector3.zero,
                Quaternion.identity,
                0
            ));
            return transforms;
        }

        // Розраховуємо оптимальні позиції з урахуванням стискання
        var layoutData = CalculateLayoutData(cardCount);

        for (int i = 0; i < cardCount; i++) {
            transforms.Add(new LayoutPoint(
                CalculateLocalPosition(i, cardCount, layoutData),
                CalculateLocalRotation(i, cardCount),
                i
            ));
        }

        return transforms;
    }

    private LayoutData CalculateLayoutData(int cardCount) {
        // Мінімальний відступ між картами (не менше 5% від ширини карти)
        float minSpacing = _settings.CardWidth * 0.05f;

        // Мінімальна загальна ширина (карти впритул одна до одної)
        float minTotalWidth = (cardCount * _settings.CardWidth) + ((cardCount - 1) * minSpacing);

        // Якщо навіть мінімальна ширина більша за максимальну - використовуємо мінімальний відступ
        if (minTotalWidth > _settings.MaxHandWidth) {
            // У цьому випадку карти будуть частково перекриватися
            float overlapWidth = _settings.MaxHandWidth;
            float overlapSpacing = (overlapWidth - (cardCount * _settings.CardWidth)) / (cardCount - 1);

            float startX_inner = -overlapWidth / 2f + _settings.CardWidth / 2f;

            return new LayoutData {
                StartX = startX_inner,
                Spacing = _settings.CardWidth + overlapSpacing,
                IsOverlapping = true,
                OverlapAmount = Mathf.Abs(overlapSpacing) / _settings.CardWidth
            };
        }

        // Ідеальна ширина без стиснення
        float idealTotalWidth = (cardCount * _settings.CardWidth) + ((cardCount - 1) * _settings.CardSpacing);

        // Якщо ідеальна ширина не перевищує максимальну - використовуємо ідеальну
        if (idealTotalWidth <= _settings.MaxHandWidth) {
            float startX_inner = -((cardCount - 1) * (_settings.CardWidth + _settings.CardSpacing)) / 2f;

            return new LayoutData {
                StartX = startX_inner,
                Spacing = _settings.CardWidth + _settings.CardSpacing,
                CompressionRatio = 1f
            };
        }

        // Потрібно стискати тільки відступи
        float availableWidth = _settings.MaxHandWidth;
        float compressedSpacing = (availableWidth - (cardCount * _settings.CardWidth)) / (cardCount - 1);
        compressedSpacing = Mathf.Max(compressedSpacing, minSpacing);

        float actualTotalWidth = (cardCount * _settings.CardWidth) + ((cardCount - 1) * compressedSpacing);
        float startX = -actualTotalWidth / 2f + _settings.CardWidth / 2f;
        float compressionRatio = actualTotalWidth / idealTotalWidth;

        return new LayoutData {
            StartX = startX,
            Spacing = _settings.CardWidth + compressedSpacing,
            CompressionRatio = compressionRatio
        };
    }

    private Vector3 CalculateLocalPosition(int index, int totalCards, LayoutData layoutData) {
        float xPos = layoutData.StartX + index * layoutData.Spacing;

        // Додаємо нелінійне зміщення по Y та Z для 3D ефекту
        float normalizedIndex = (float)index / (totalCards - 1);
        float yPos = -normalizedIndex * _settings.DepthOffset;
        float zPos = -normalizedIndex * _settings.VerticalOffset;

        // Додаємо варіацію для більш природнього вигляду
        if (_settings.PositionVariation > 0f) {
            float seed = index * 12.9898f;
            float randomX = (Mathf.Sin(seed) * 2f - 1f) * _settings.PositionVariation;
            float randomY = (Mathf.Sin(seed * 1.5f) * 2f - 1f) * _settings.PositionVariation * 0.1f;

            xPos += randomX;
            yPos += randomY;
        }

        return new Vector3(xPos, yPos, zPos);
    }

    private Quaternion CalculateLocalRotation(int index, int totalCards) {
        if (totalCards <= 1) return Quaternion.identity;

        float t = (float)index / (totalCards - 1);
        float angle = Mathf.Lerp(-_settings.MaxRotationAngle, _settings.MaxRotationAngle, t);

        if (_settings.RotationOffset > 0f) {
            float seed = index * 23.1406f;
            float randomOffset = (Mathf.Sin(seed) * 2f - 1f) * _settings.RotationOffset;
            angle += randomOffset;
        }

        return Quaternion.Euler(0f, angle, 0f);
    }

    // Допоміжний клас для зберігання розрахованих даних макету
    private class LayoutData {
        public float StartX;
        public float Spacing;
        public float CompressionRatio = 1f;
        public bool IsOverlapping = false;
        public float OverlapAmount = 0f;
    }
}

public class SummonZone3DLayout : ILayout3DHandler {
    private readonly SummonZone3DLayoutSettings _settings;

    public SummonZone3DLayout(SummonZone3DLayoutSettings settings) {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    public List<LayoutPoint> CalculateCardTransforms(int cardCount) {
        var transforms = new List<LayoutPoint>();

        if (cardCount <= 0) return transforms;

        // Одна карта завжди в центрі (0, 0, 0)
        if (cardCount == 1) {
            transforms.Add(new LayoutPoint(Vector3.zero, Quaternion.identity, 0));
            return transforms;
        }

        // Якщо використовуємо декілька рядів і карт багато
        if (_settings.UseMultipleRows && cardCount > _settings.MaxCardsPerRow) {
            return CalculateMultiRowLayout(cardCount);
        } else {
            return CalculateSingleRowLayout(cardCount);
        }
    }

    private List<LayoutPoint> CalculateSingleRowLayout(int cardCount) {
        var transforms = new List<LayoutPoint>();

        // Загальна ширина всіх карт + відступи між ними
        float totalWidth = (cardCount * _settings.CardWidth) + ((cardCount - 1) * _settings.CardSpacing);

        // Стартова позиція (ліва межа)
        float startX = -totalWidth / 2f + _settings.CardWidth / 2f;

        // Крок між центрами карт
        float step = _settings.CardWidth + _settings.CardSpacing;

        for (int i = 0; i < cardCount; i++) {
            float xPos = startX + i * step;
            Vector3 position = new Vector3(xPos, 0f, 0f);

            transforms.Add(new LayoutPoint(position, Quaternion.identity, i));
        }

        return transforms;
    }

    private List<LayoutPoint> CalculateMultiRowLayout(int cardCount) {
        var transforms = new List<LayoutPoint>();

        int rows = Mathf.CeilToInt((float)cardCount / _settings.MaxCardsPerRow);
        int cardIndex = 0;

        for (int row = 0; row < rows; row++) {
            // Кількість карт в поточному ряду
            int cardsInRow = Mathf.Min(_settings.MaxCardsPerRow, cardCount - cardIndex);

            // Z позиція для поточного ряду (центруємо ряди відносно Z = 0)
            float zPos = CalculateRowZPosition(row, rows);

            // Розраховуємо позиції для карт в поточному ряду
            var rowPositions = CalculateRowPositions(cardsInRow, zPos);

            for (int i = 0; i < cardsInRow; i++) {
                transforms.Add(new LayoutPoint(rowPositions[i], Quaternion.identity, cardIndex));
                cardIndex++;
            }
        }

        return transforms;
    }

    private float CalculateRowZPosition(int currentRow, int totalRows) {
        if (totalRows == 1) return 0f;

        // Центруємо ряди відносно Z = 0
        float totalRowsHeight = (totalRows - 1) * _settings.RowSpacing;
        float startZ = -totalRowsHeight / 2f;

        return startZ + currentRow * _settings.RowSpacing;
    }

    private List<Vector3> CalculateRowPositions(int cardsInRow, float zPos) {
        var positions = new List<Vector3>();

        if (cardsInRow == 1) {
            positions.Add(new Vector3(0f, 0f, zPos));
            return positions;
        }

        // Загальна ширина ряду
        float totalWidth = (cardsInRow * _settings.CardWidth) + ((cardsInRow - 1) * _settings.CardSpacing);

        // Стартова позиція
        float startX = -totalWidth / 2f + _settings.CardWidth / 2f;

        // Крок між центрами карт
        float step = _settings.CardWidth + _settings.CardSpacing;

        for (int i = 0; i < cardsInRow; i++) {
            float xPos = startX + i * step;
            positions.Add(new Vector3(xPos, 0f, zPos));
        }

        return positions;
    }
}