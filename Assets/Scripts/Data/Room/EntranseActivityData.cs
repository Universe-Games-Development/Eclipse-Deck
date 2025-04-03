using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "EntranceActivityData", menuName = "Map/Activities/EntranceActivityData")]
public class EntranseActivityData : ActivityData {

    public override RoomActivity CreateActivity(DiContainer diContainer) {
        return diContainer.Instantiate<EntranceRoomActivity>();
    }
}

public class EntranceRoomActivity : RoomActivity {
    public override void Initialize(Room room) {
        Debug.Log("Enterance effects");
        CompleteActivity();
    }
}
