using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "BossActivityData", menuName = "Map/Activities/BossActivityData")]
public class BossActivityData : ActivityData {

    public override RoomActivity CreateActivity(DiContainer diContainer) {
        return new BossRoomActivity();
    }
}

public class BossRoomActivity : RoomActivity {
    public override void Initialize(Room room) {
        Debug.Log("Exit effects");
        CompleteActivity();
    }
}
