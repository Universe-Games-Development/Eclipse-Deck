using UnityEngine;

public class TableGameEntitySO : ScriptableObject {
    [Header("UI Data")]
    public string Name;
    public string description;
    public Sprite characterSprite;

    [Header("Data")]
    public int attack;
    public int health;
}
