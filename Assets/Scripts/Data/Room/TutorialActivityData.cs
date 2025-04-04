using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "TutorialActivityData", menuName = "Map/Activities/TutorialActivityData")]
public class TutorialActivityData : ActivityData {

    public override RoomActivity CreateActivity(DiContainer diContainer) {
        return diContainer.Instantiate<TutorialRoomActivity>();
    }
}

public class TutorialRoomActivity : EnemyRoomActivity{

    protected override async UniTask<bool> SpawnEnemy() {
        return await _enemySpawner.SpawnEnemy(EnemyType.Tutorial);
    }
}