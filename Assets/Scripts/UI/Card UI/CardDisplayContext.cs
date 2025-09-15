using UnityEngine;

public struct CardDisplayContext {
    public CardDisplayData Data;
    public CardDisplayConfig Config;

    public CardDisplayContext(CardDisplayData data, CardDisplayConfig config) {
        Data = data;
        Config = config;
    }
}



[System.Serializable]
public class CardDisplayConfig {
    public bool showCost = true;
    public bool showName = true;
    public bool showStats = false;
    public bool showFrame = true;
    public bool showCategory = true;
    public bool showRarity = true;

    // Додаткові параметри
    public float scaleMultiplier = 1f;
    public bool interactable = true;
    public bool showTooltips = true;

    // Пресети
    public static CardDisplayConfig ForCreature() => new() {
        showCost = false,
        showStats = true,
        showFrame = false,
        showName = false,
        showCategory = false,
        showRarity = false,
        interactable = false
    };

    public static CardDisplayConfig ForHandCard() => new() {
        showCost = true,
        showStats = true,
        showFrame = true,
        showName = true,
        showCategory = true,
        showRarity = true,
        interactable = true
    };

    public static CardDisplayConfig ForZoomedView() => new() {
        showCost = true,
        showStats = true,
        showFrame = true,
        showName = true,
        showCategory = true,
        showRarity = true,
        scaleMultiplier = 1.5f,
        interactable = false
    };
}

[System.Serializable]
public class CardDisplayData {
    public string name;
    public int cost;
    public int attack;
    public int health;
    public Sprite portrait;
    public Sprite background;
    public Color rarity;
}
