using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "EnemyActivityData", menuName = "Map/Activities/EnemyActivityData")]
public class EnemyActivityData : ActivityData {

    public override RoomActivity CreateActivity(DiContainer diContainer) {
        return diContainer.Instantiate<EnemyRoomActivity>();
    }
}

public class EnemyRoomActivity : RoomActivity {
    [Inject] private EnemySpawner _enemySpawner;
    [Inject] private GameEventBus _eventBus;

    public EnemyRoomActivity() {
        _blocksRoomClear = true;
    }

    public override void Initialize(Room room) {
        _eventBus.SubscribeTo<BattleEndEventData>(HandleBattleEnd);
        _enemySpawner.PrepareEnemy();
    }

    private void HandleBattleEnd(ref BattleEndEventData eventData) {
        CompleteActivity();
    }

    public override void Dispose() {
        _eventBus.UnsubscribeFrom<BattleEndEventData>(HandleBattleEnd);
    }
}