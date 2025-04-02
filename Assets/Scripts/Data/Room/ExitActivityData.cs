using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "ExitActivityData", menuName = "Map/Activities/ExitActivityData")]
public class ExitActivityData : ActivityData {

    public override RoomActivity CreateActivity(DiContainer diContainer) {
        return new ExitRoomActivity();
    }
}
public class ExitRoomActivity : RoomActivity {
    public override void Initialize(Room room) {
        Debug.Log("Exit effects");
        CompleteActivity();
    }
}
