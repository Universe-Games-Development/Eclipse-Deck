using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemy", menuName = "Opponents/")]
public class EnemySO : ScriptableObject {
    public string Name;
    public int Health;
    public GameObject Prefab;
    public CardCollectionSO collection;
    public SpeechSO speech;
}
