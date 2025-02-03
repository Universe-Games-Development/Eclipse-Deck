using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "Card", menuName = "TGE/Card")]
public class CardSO : TableGameEntitySO {
    [Header("Card UI")]
    public Sprite biomeIamge;
    public string AuthorName;

    [Header("Logic")]
    public string resourseId;
    public Rarity rarity;
    public CardType cardType;
    public int cost;

    [Header ("Not creature Abilities!")]
    public List<CardAbilitySO> cardAbilities;

    // Soon be separated
    [Header("Creature")]
    public CreatureSO creatureData;


    private static readonly Dictionary<Rarity, Color> rarityColors = new Dictionary<Rarity, Color> {
        { Rarity.Common, Color.gray },
        { Rarity.Uncommon, Color.green },
        { Rarity.Rare, Color.blue },
        { Rarity.Epic, new Color(0.58f, 0, 0.83f) },
        { Rarity.Legendary, new Color(1f, 0.5f, 0f) }
    };

    private void OnValidate() {
        if (string.IsNullOrEmpty(resourseId)) {
            resourseId = System.Guid.NewGuid().ToString();
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