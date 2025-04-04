using UnityEngine;

[CreateAssetMenu(fileName = "OpponentData", menuName = "Opponents/OpponentData")]
public class OpponentData : ScriptableObject {
    public string Name;
    public Sprite Sprite;
    public GameObject ViewModel;

    public int Health;
    public int Mana;
    public CardCollectionSO collection;

    public bool isFlying;
    public EnemyType enemyType;

    public SpeechData speechData;
   
}