using System;
using UnityEngine;

public enum LogLevel {
    Debug = 0,
    Info = 1,
    Warning = 2,
    Error = 3
}

[Flags]
public enum LogCategory {
    None = 0,
    OperationManager = 1 << 0,
    TargetsFiller = 1 << 1,
    GameLogic = 1 << 2,
    UI = 1 << 3,
    Network = 1 << 4,
    Audio = 1 << 5,
    Animation = 1 << 6,
    AI = 1 << 7,
    Physics = 1 << 8,
    CardModule = 1 << 9,
    Visualmanager = 1 << 10,
    All = ~0,

}


