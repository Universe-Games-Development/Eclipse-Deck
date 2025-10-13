using UnityEngine;

[CreateAssetMenu(fileName = "OpponentData", menuName = "TGE/Opponent")]
public class OpponentData : ScriptableObject {
    [Header("Presentation")]
    [Tooltip("Усі вороги/гравці мають View на базі OpponentView")]
    public CharacterPresenter presenterPrefab;

    [Header("Stats")]
    public string Name;
    public Sprite Sprite;
    public int Health;
    public int Mana;
    public CardCollectionSO collection;
    public bool isFlying;
    public SpeechData speechData;

    public Color Color;
}

