using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "ShopActivityData", menuName = "Map/Activities/ShopActivityData")]
public class ShopActivityData : ActivityData {

    public override RoomActivity CreateActivity(DiContainer diContainer) {
        return diContainer.Instantiate<ShopRoomActivity>();
    }
}

public class ShopRoomActivity : RoomActivity {
    public override async UniTask Initialize(Room room) {
        Debug.Log("Spawning shop");
        CompleteActivity();
        await UniTask.CompletedTask;
    }
}
