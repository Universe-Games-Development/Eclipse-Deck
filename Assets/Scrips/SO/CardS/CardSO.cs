using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "New Card", menuName = "Cards/Card")]
public class CardSO : ScriptableObject {
    [Header("UI")]
    public string Name;
    public string description;
    public Sprite characterSprite;
    public Sprite biomeIamge;
    public string AuthorName;

    [Header("Logic")]
    public string id;
    public Rarity rarity;
    public CardType cardType;
    public int cost;
    public int attack;
    public int health;
    public List<CardAbilitySO> abilities;

    private static readonly Dictionary<Rarity, Color> rarityColors = new Dictionary<Rarity, Color> {
        { Rarity.Common, Color.gray },
        { Rarity.Uncommon, Color.green },
        { Rarity.Rare, Color.blue },
        { Rarity.Epic, new Color(0.58f, 0, 0.83f) },
        { Rarity.Legendary, new Color(1f, 0.5f, 0f) }
    };

    private void OnValidate() {
        if (string.IsNullOrEmpty(id)) {
            id = System.Guid.NewGuid().ToString();
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }
    }

    public Color GetRarityColor() {
        if (rarityColors.TryGetValue(rarity, out var color)) {
            return color;
        }

        return Color.white;
    }
}

public enum CardType {
    Spell,
    Creature,
    Trap
}