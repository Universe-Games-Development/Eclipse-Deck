using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public abstract class CardData : ScriptableObject {
    [Header("Global Card Settings")]
    public int MAX_CARDS_COST = 30;
    public CardType cardType;

    [Header("Card UI")]
    public string Name;
    public string Description;
    public string AuthorName;
    public Sprite CharacterSprite;
    public Sprite biomeIamge;

    [Header("Logic")]
    public string resourseId;
    public Rarity rarity;
    public int cost;
    public float spawnChance;
    public CreatureView creatureViewPrefab;

    private static readonly Dictionary<Rarity, Color> rarityColors = new Dictionary<Rarity, Color> {
        { Rarity.Common, Color.gray },
        { Rarity.Uncommon, Color.green },
        { Rarity.Rare, Color.blue },
        { Rarity.Epic, new Color(0.58f, 0, 0.83f) },
        { Rarity.Legendary, new Color(1f, 0.5f, 0f) }
    };


    private static readonly Dictionary<Rarity, float> raritySpawnChances = new Dictionary<Rarity, float> {
    { Rarity.Common, 0.5f },
    { Rarity.Uncommon, 0.3f },
    { Rarity.Rare, 0.15f },
    { Rarity.Epic, 0.04f },
    { Rarity.Legendary, 0.01f }
    };

    private void OnValidate() {
        if (string.IsNullOrEmpty(resourseId)) {
            resourseId = System.Guid.NewGuid().ToString();
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        // Update spawn chance based on rarity
        UpdateSpawnChance();
    }

    private void UpdateSpawnChance() {
        if (raritySpawnChances.TryGetValue(rarity, out var chance)) {
            spawnChance = chance;
        } else {
            spawnChance = 0f; // Default if rarity doesn't match
        }
    }

    public Color GetRarityColor() {
        if (rarityColors.TryGetValue(rarity, out var color)) {
            return color;
        }
        return Color.white;
    }
}
