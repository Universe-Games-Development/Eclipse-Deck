using System;
using UnityEngine;
using Zenject;

public abstract class RoomActivity : IDisposable {
    protected bool _blocksRoomClear = false;
    public bool BlocksRoomClear => _blocksRoomClear;
    public Action<bool> OnActivityCompleted;
    public abstract void Initialize(Room room);
    public virtual void Dispose() { }
}

public class EnemyRoomActivity : RoomActivity {
    [Inject] private EnemySpawner _enemySpawner;
    [Inject] private GameEventBus _eventBus;

    public EnemyRoomActivity() {
        _blocksRoomClear = true;
    }

    public override void Initialize(Room room) {
        _eventBus.SubscribeTo<BattleEndEventData>(HandleBattleEnd);
        _enemySpawner.SpawnEnemy(room.Data.type);
    }

    private void HandleBattleEnd(ref BattleEndEventData eventData) {
        CompleteActivity(eventData.Looser is Enemy);
    }

    public void CompleteActivity(bool playerWon) {
        OnActivityCompleted?.Invoke(playerWon);
    }

    public override void Dispose() {
        _eventBus.UnsubscribeFrom<BattleEndEventData>(HandleBattleEnd);
    }
}
