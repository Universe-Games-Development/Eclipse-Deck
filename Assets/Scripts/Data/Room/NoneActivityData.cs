﻿using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "NoneActivityData", menuName = "Map/Activities/NoneActivityData")]
public class NoneActivityData : ActivityData {

    public override RoomActivity CreateActivity(DiContainer diContainer) {
        return diContainer.Instantiate<NoneRoomActivity>();
    }
}

public class NoneRoomActivity : RoomActivity {
    public override async UniTask Initialize(Room room) {
        CompleteActivity();
        await UniTask.CompletedTask;
    }
}