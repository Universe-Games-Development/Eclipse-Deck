using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DeckConfig", menuName = "TGE/DeckCollection")]
public class DeckConfiguration : ScriptableObject {
    public List<CardEntry> Cards;

    public bool UseRandomGeneration = true;
    public int RandomCardCount = 20;
}

[Serializable]
public class CardEntry {
    public CardData CardData;
    public int Quantity = 1;
}
