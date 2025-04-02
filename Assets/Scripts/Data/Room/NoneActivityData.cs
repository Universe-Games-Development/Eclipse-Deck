using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "NoneActivityData", menuName = "Map/Activities/NoneActivityData")]
public class NoneActivityData : ActivityData {

    public override RoomActivity CreateActivity(DiContainer diContainer) {
        return new NoneRoomActivity();
    }
}

public class NoneRoomActivity : RoomActivity {
    public override void Initialize(Room room) {
        CompleteActivity();
    }
}