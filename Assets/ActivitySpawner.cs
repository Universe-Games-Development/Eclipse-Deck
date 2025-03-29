using System;
using UnityEngine;
using Zenject;

public class ActivitySpawner : MonoBehaviour {
    [SerializeField] private Transform spawnPoint;

    [Inject] DiContainer _container;

    public RoomActivity GetActivity(Room room) {
        RoomActivity activity  = null;
        switch (room.Data.type) {
            case RoomType.Enemy:
            case RoomType.Boss:
                SpawnEnemy(room);
                break;
            default:
                break;
        }
        return activity;
    }

    public RoomActivity SpawnEnemy(Room room) {
        var enemyActivity = _container.Instantiate<EnemyRoomActivity>(new object[] { spawnPoint, room });
        return enemyActivity;
    }
}

public abstract class RoomActivity : IDisposable {
    protected bool _blocksRoomClear = false;
    public bool BlocksRoomClear => _blocksRoomClear;
    public Action<bool> OnActivityCompleted;
    protected Transform _spawnPoint;
    protected Room _room;

    public RoomActivity(Transform spawnPoint, Room room) {
        _spawnPoint = spawnPoint;
        _room = room;
    }

    public abstract void Initialize();
    public virtual void Dispose() { }
}

public class EnemyRoomActivity : RoomActivity {
    [Inject] private EnemyProvider _enemyProvider;
    [Inject] private IEnemyFactory _enemyFactory;
    [Inject] private EnemyPresenter _enemyPresenter;
    private GameEventBus _eventBus;
    private Enemy _currentEnemy;

    public EnemyRoomActivity(GameEventBus eventBus, Transform spawnPoint, Room room) : base(spawnPoint, room) {
        _blocksRoomClear = true;
        _eventBus = eventBus;
    }

    public override void Initialize() {
        _eventBus.SubscribeTo<BattleEndEventData>(HandleBattleEnd);
        OpponentData opponentData = null;

        switch (_room.Data.type) {
            case RoomType.Boss:
                opponentData = _enemyProvider.GetBossData();
                break;
            case RoomType.Enemy:
                opponentData = _enemyProvider.GetEnemyData();
                break;
            default:
                throw new ArgumentException("Invalid room type");
        }

        _currentEnemy = _enemyFactory.Create(opponentData);
        _enemyPresenter.InitializeEnemy(_currentEnemy, _spawnPoint);
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
