using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;

public abstract class GameOperation : IExecutableTask {
    public UniTask<bool> ExecuteAsync() {
        return UniTask.FromResult(Execute());
    }

    public abstract bool Execute();
}


