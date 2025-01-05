using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCardCollection", menuName = "Cards/Collection")]
public class CardCollectionSO : ScriptableObject
{
    [Header("Collection Details")] 
    public string collectionName; 
    public string description;
    public List<CardSO> cards;
}
