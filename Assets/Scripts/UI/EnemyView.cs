using UnityEngine;

public class EnemyView : MonoBehaviour {
    [SerializeField] private Animator animator;
    private OpponentData enemyData;
    
    
    public void Initialize(OpponentData enemyData) {
        this.enemyData = enemyData;
        // Soon we will use this data to display the enemy's name and other information
    }
}
