using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "ExitActivityData", menuName = "Map/Activities/ExitActivityData")]
public class ExitActivityData : ActivityData {

    public override RoomActivity CreateActivity(DiContainer diContainer) {
        return diContainer.Instantiate<ExitRoomActivity>();
    }
}
public class ExitRoomActivity : RoomActivity {
    public override async UniTask Initialize(Room room) {
        Debug.Log("Exit effects");
        CompleteActivity();
        await UniTask.CompletedTask;
    }
}
