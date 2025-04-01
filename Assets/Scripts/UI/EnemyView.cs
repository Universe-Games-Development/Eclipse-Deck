using UnityEngine;

public class EnemyView : MonoBehaviour {
    [SerializeField] private Animator animator;
    private OpponentData enemyData;
    private GameObject ViewModel;
    
    public void Initialize(OpponentData enemyData) {
        this.enemyData = enemyData;
        if (ViewModel != null) {
            Destroy(ViewModel);
        }
        ViewModel = Instantiate(enemyData.ViewModel, gameObject.transform);
        // Soon we will use this data to display the enemy's name and other information
    }
}
