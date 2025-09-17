using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public abstract class CardData : ScriptableObject {
    [Header("Global Card Settings")]
    public int cost;

    [Header("Card UI")]
    public string Name;
    public string Description;
    public string AuthorName;
    public Sprite Portait;
    public Sprite Background;

    [Header("Logic")]
    public string resourseId;
    public Rarity Rarity;
   

    [Header("Rarity Info (Auto-Generated)")]
    [SerializeField] private float spawnChance; // Тепер приватне, оновлюється автоматично
    [SerializeField] private Color rarityColor; // Показуємо колір в інспекторі для наглядності
    [SerializeField] private string rarityDisplayName; // Локалізована назва

    [Header("Operations")]
    public List<OperationData> operationsData = new List<OperationData>();

    private void OnValidate() {
        // Генеруємо унікальний ID якщо потрібно
        if (string.IsNullOrEmpty(resourseId)) {
            resourseId = System.Guid.NewGuid().ToString();
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        // Оновлюємо дані рідкості через утиліту
        UpdateRarityData();
    }

    private void UpdateRarityData() {
        spawnChance = RarityUtility.GetSpawnChance(Rarity);
        rarityColor = RarityUtility.GetRarityColor(Rarity);
        rarityDisplayName = RarityUtility.GetRarityDisplayName(Rarity);
    }

}
