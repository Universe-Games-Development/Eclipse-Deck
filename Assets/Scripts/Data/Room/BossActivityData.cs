using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "BossActivityData", menuName = "Map/Activities/BossActivityData")]
public class BossActivityData : ActivityData {

    public override RoomActivity CreateActivity(DiContainer diContainer) {
        return diContainer.Instantiate<BossRoomActivity>();
    }
}

public class BossRoomActivity : EnemyRoomActivity {
    protected override bool TrySpawnEnemy(out Enemy enemy) {
        return _enemyManager.TrySpawnBoss(out enemy);
    }
}
