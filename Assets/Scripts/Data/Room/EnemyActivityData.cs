using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "EnemyActivityData", menuName = "Map/Activities/EnemyActivityData")]
public class EnemyActivityData : ActivityData {

    public override RoomActivity CreateActivity(DiContainer diContainer) {
        return diContainer.Instantiate<EnemyRoomActivity>();
    }
}

public class EnemyRoomActivity : RoomActivity {
    [Inject] protected EnemyFactory _enemySpawner;
    [Inject] protected IEventBus<IEvent> _eventBus;
    private Enemy _currentEnemy;

    public EnemyRoomActivity() {
        _blocksRoomClear = true;
    }

    public override void Initialize(Room room) {
        Opponent opponent = SpawnEnemy();
        // Создаем врага при инициализации активности
        if (opponent == null) {
            Debug.LogWarning("No enemy to spawn. Clearing room...");
            _blocksRoomClear = false;
            CompleteActivity();
        } else {
            _eventBus.SubscribeTo<BattleEndEventData>(HandleBattleEnd);
            _eventBus.SubscribeTo<EnemyDefeatedEvent>(HandleEnemyDefeated);
        }
    }

    protected virtual Enemy SpawnEnemy() {
        return _enemySpawner.CreateEnemy(EnemyType.Regular);
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
        if (_eventBus != null) {
            _eventBus.UnsubscribeFrom<BattleEndEventData>(HandleBattleEnd);
            _eventBus.UnsubscribeFrom<EnemyDefeatedEvent>(HandleEnemyDefeated);
        }
        _currentEnemy = null;
    }
}

public struct EnemyDefeatedEvent : IEvent {
    public Opponent Opponent { get; }
    public EnemyDefeatedEvent(Opponent opponent) {
        Opponent = opponent;
    }
}