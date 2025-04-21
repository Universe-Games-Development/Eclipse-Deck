using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

public class EnemyPresenter : BaseOpponentPresenter {
    public Enemy Enemy => (Enemy) Model;
    private EnemyView EnemyView => (EnemyView) View;

    

    public EnemyPresenter(Enemy enemy, OpponentView view) : base(enemy, view) {
        enemy.OnSpawned += StartEnemyActivity;
    }

    public async UniTask StartEnemyActivity() {
        await EnemyView.PlayAppearAnimation();
        
    }
}