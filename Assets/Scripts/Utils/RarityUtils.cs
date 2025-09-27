using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class RarityUtility {
    // Структура для опису рівня рідкості
    [System.Serializable]
    public struct RarityLevel {
        public Rarity rarity;
        public string displayName;
        public float rarityValue; // 0-1, де 0 = найчастіше, 1 = найрідкісніше
        public float spawnChance;
        public Color color;

        public RarityLevel(Rarity rarity, string displayName, float rarityValue, float spawnChance, Color color) {
            this.rarity = rarity;
            this.displayName = displayName;
            this.rarityValue = rarityValue;
            this.spawnChance = spawnChance;
            this.color = color;
        }
    }

    // Налаштування рівнів рідкості
    private static readonly Dictionary<Rarity, RarityLevel> rarityLevels = new Dictionary<Rarity, RarityLevel>
    {
        { Rarity.Common, new RarityLevel(Rarity.Common, "Звичайна", 0f, 0.5f, Color.gray) },
        { Rarity.Uncommon, new RarityLevel(Rarity.Uncommon, "Незвичайна", 0.25f, 0.3f, Color.green) },
        { Rarity.Rare, new RarityLevel(Rarity.Rare, "Рідкісна", 0.5f, 0.15f, Color.blue) },
        { Rarity.Epic, new RarityLevel(Rarity.Epic, "Епічна", 0.75f, 0.04f, new Color(0.58f, 0, 0.83f)) },
        { Rarity.Legendary, new RarityLevel(Rarity.Legendary, "Легендарна", 1f, 0.01f, new Color(1f, 0.5f, 0f)) }
    };

    // Градієнт для автоматичного визначення кольорів
    private static readonly Gradient rarityGradient = CreateRarityGradient();

    #region Основні методи

    /// <summary>
    /// Отримати колір за рідкістю
    /// </summary>
    public static Color GetRarityColor(Rarity rarity) {
        if (rarityLevels.TryGetValue(rarity, out var level)) {
            return level.color;
        }
        return Color.white;
    }

    /// <summary>
    /// Отримати колір за значенням рідкості (0-1) використовуючи градієнт
    /// </summary>
    public static Color GetRarityColorFromValue(float rarityValue) {
        return rarityGradient.Evaluate(Mathf.Clamp01(rarityValue));
    }

    /// <summary>
    /// Отримати шанс спавну за рідкістю
    /// </summary>
    public static float GetSpawnChance(Rarity rarity) {
        if (rarityLevels.TryGetValue(rarity, out var level)) {
            return level.spawnChance;
        }
        return 0f;
    }

    /// <summary>
    /// Отримати значення рідкості (0-1)
    /// </summary>
    public static float GetRarityValue(Rarity rarity) {
        if (rarityLevels.TryGetValue(rarity, out var level)) {
            return level.rarityValue;
        }
        return 0f;
    }

    /// <summary>
    /// Отримати локалізовану назву рідкості
    /// </summary>
    public static string GetRarityDisplayName(Rarity rarity) {
        if (rarityLevels.TryGetValue(rarity, out var level)) {
            return level.displayName;
        }
        return rarity.ToString();
    }

    /// <summary>
    /// Отримати всю інформацію про рідкість
    /// </summary>
    public static RarityLevel GetRarityLevel(Rarity rarity) {
        rarityLevels.TryGetValue(rarity, out var level);
        return level;
    }

    #endregion

    #region Розширені методи

    /// <summary>
    /// Визначити рідкість за значенням (0-1)
    /// </summary>
    public static Rarity GetRarityFromValue(float value) {
        value = Mathf.Clamp01(value);

        var sortedLevels = rarityLevels.Values.OrderBy(x => x.rarityValue).ToArray();

        for (int i = 0; i < sortedLevels.Length; i++) {
            if (value <= sortedLevels[i].rarityValue) {
                return sortedLevels[i].rarity;
            }
        }

        return sortedLevels.Last().rarity;
    }

    /// <summary>
    /// Згенерувати випадкову рідкість на основі шансів спавну
    /// </summary>
    public static Rarity GenerateRandomRarity() {
        float randomValue = Random.value;
        float cumulativeChance = 0f;

        // Сортуємо за спаданням шансу (Common першим)
        var sortedByChance = rarityLevels.Values.OrderByDescending(x => x.spawnChance);

        foreach (var level in sortedByChance) {
            cumulativeChance += level.spawnChance;
            if (randomValue <= cumulativeChance) {
                return level.rarity;
            }
        }

        return Rarity.Common; // Fallback
    }

    /// <summary>
    /// Створити градієнт кольорів на основі рідкості
    /// </summary>
    public static Gradient CreateCustomGradient(params (Rarity rarity, Color color)[] customColors) {
        var gradient = new Gradient();
        var colorKeys = new GradientColorKey[customColors.Length];
        var alphaKeys = new GradientAlphaKey[customColors.Length];

        for (int i = 0; i < customColors.Length; i++) {
            float rarityValue = GetRarityValue(customColors[i].rarity);
            colorKeys[i] = new GradientColorKey(customColors[i].color, rarityValue);
            alphaKeys[i] = new GradientAlphaKey(1f, rarityValue);
        }

        gradient.SetKeys(colorKeys, alphaKeys);
        return gradient;
    }

    /// <summary>
    /// Отримати колір з анімованим ефектом (наприклад, для легендарних предметів)
    /// </summary>
    public static Color GetAnimatedRarityColor(Rarity rarity, float time = -1f) {
        Color baseColor = GetRarityColor(rarity);

        if (rarity == Rarity.Legendary) {
            if (time < 0) time = Time.time;
            float pulse = Mathf.Sin(time * 2f) * 0.3f + 0.7f;
            return baseColor * pulse;
        } else if (rarity == Rarity.Epic) {
            if (time < 0) time = Time.time;
            float shimmer = Mathf.Sin(time * 1.5f) * 0.2f + 0.8f;
            return Color.Lerp(baseColor, Color.white, shimmer * 0.3f);
        }

        return baseColor;
    }

    #endregion

    #region Приватні методи

    private static Gradient CreateRarityGradient() {
        var gradient = new Gradient();
        var sortedLevels = rarityLevels.Values.OrderBy(x => x.rarityValue).ToArray();

        var colorKeys = new GradientColorKey[sortedLevels.Length];
        var alphaKeys = new GradientAlphaKey[sortedLevels.Length];

        for (int i = 0; i < sortedLevels.Length; i++) {
            colorKeys[i] = new GradientColorKey(sortedLevels[i].color, sortedLevels[i].rarityValue);
            alphaKeys[i] = new GradientAlphaKey(1f, sortedLevels[i].rarityValue);
        }

        gradient.SetKeys(colorKeys, alphaKeys);
        return gradient;
    }

    #endregion
}

public enum Rarity {
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}