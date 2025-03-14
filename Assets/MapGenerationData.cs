// Клас вузла залишаємо майже без змін
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "MapGenerationData", menuName = "MapGeneration/MapGenerationData")]
public class MapGenerationData : ScriptableObject {
    public string seed = "simple_seed";

    [Header("Level Settings")]
    public int levelCount = 4;
    [Header("Nodes Level Settings")]
    public int initialNodesPerLevel = 4;
    public int minNodesPerLevel = 3;
    public int maxNodesPerLevel = 5;
    [Header("Connections Settings")]
    [Range(0f, 1f)] public float randomConnectionChance = 0.3f;
    [Range(0f, 1f)] public float destroyConnectionChance = 0.5f;

    public void OnValidate() {
        // Забезпечуємо, щоб максимальна кількість вузлів не була меншою за початкову
        if (initialNodesPerLevel > maxNodesPerLevel) {
            maxNodesPerLevel = initialNodesPerLevel;
        }

        // Забезпечуємо, щоб максимальна кількість вузлів була не меншою за мінімальну
        if (maxNodesPerLevel < minNodesPerLevel) {
            maxNodesPerLevel = minNodesPerLevel + 1;
        }

        // Перевірка, щоб кількість рівнів була позитивною
        if (levelCount < 1) {
            levelCount = 1;
        }

        // Перевірка, щоб шанси на створення та знищення з'єднання були в межах [0, 1]
        randomConnectionChance = Mathf.Clamp(randomConnectionChance, 0f, 1f);
        destroyConnectionChance = Mathf.Clamp(destroyConnectionChance, 0f, 1f);

        // Попередження про некоректну кількість вузлів
        if (minNodesPerLevel < 1) {
            Debug.LogWarning("Minimum nodes per level cannot be less than 1. Resetting to 1.");
            minNodesPerLevel = 1;
        }

        // Попередження про пустий seed
        if (string.IsNullOrEmpty(seed)) {
            Debug.LogWarning("Seed is empty. Using default value 'default_seed'.");
            seed = "default_seed";
        }
    }

}
