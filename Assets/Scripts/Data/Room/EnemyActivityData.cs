using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "EnemyActivityData", menuName = "Map/Activities/EnemyActivityData")]
public class EnemyActivityData : ActivityData {

    public override RoomActivity CreateActivity(DiContainer diContainer) {
        return diContainer.Instantiate<EnemyRoomActivity>();
    }
}

public class EnemyRoomActivity : RoomActivity {
    [Inject] protected EnemyManager _enemyManager;
    [Inject] protected GameEventBus _eventBus;

    private Enemy _currentEnemy;

    public EnemyRoomActivity() {
        _blocksRoomClear = true;
    }

    public override void Initialize(Room room) {
        // Создаем врага при инициализации активности
        if (!TrySpawnEnemy(out _currentEnemy)) {
            Debug.LogWarning("No enemy to spawn. Clearing room...");
            _blocksRoomClear = false;
            CompleteActivity();
        } else {
            _eventBus.SubscribeTo<BattleEndEventData>(HandleBattleEnd);
            _eventBus.SubscribeTo<EnemyDefeatedEvent>(HandleEnemyDefeated);
        }
    }

    protected virtual bool TrySpawnEnemy(out Enemy enemy) {
        return _enemyManager.TrySpawnRegularEnemy(out enemy);
    }

    private void HandleBattleEnd(ref BattleEndEventData eventData) {
        // Завершаем активность при окончании боя
        CompleteActivity();
    }

    private void HandleEnemyDefeated(ref EnemyDefeatedEvent eventData) {
        // Проверяем, что это именно наш враг
        if (_currentEnemy != null && eventData.Opponent == _currentEnemy) {
            CompleteActivity();
        }
    }

    public override void Dispose() {
        _eventBus.UnsubscribeFrom<BattleEndEventData>(HandleBattleEnd);
        _eventBus.UnsubscribeFrom<EnemyDefeatedEvent>(HandleEnemyDefeated);
        _currentEnemy = null;
    }
}

public struct EnemyDefeatedEvent : IEvent {
    public Opponent Opponent { get; }
    public EnemyDefeatedEvent(Opponent opponent) {
        Opponent = opponent;
    }
}