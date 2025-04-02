using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "ShopActivityData", menuName = "Map/Activities/ShopActivityData")]
public class ShopActivityData : ActivityData {

    public override RoomActivity CreateActivity(DiContainer diContainer) {
        return new ShopRoomActivity();
    }
}

public class ShopRoomActivity : RoomActivity {
    public override void Initialize(Room room) {
        Debug.Log("Spawning shop");
        CompleteActivity();
    }
}
