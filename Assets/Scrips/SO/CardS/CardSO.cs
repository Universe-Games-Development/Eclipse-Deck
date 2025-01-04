using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "New Card", menuName = "Cards/Card")]
public class CardSO : ScriptableObject {
    [Header("UI")]
    public string cardName;
    public string description;
    public Sprite mainImage;

    [Header("Logic")]
    public string id;
    public Rarity rarity;
    public CardType cardType;
    public int cost;
    public int attack;
    public int health;
    public AbilitySO[] abilities;

    private void OnValidate() {
        if (string.IsNullOrEmpty(id)) {
            id = System.Guid.NewGuid().ToString();
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }
    }
}

public enum CardType {
    Spell,
    Creature,
    Trap
}
