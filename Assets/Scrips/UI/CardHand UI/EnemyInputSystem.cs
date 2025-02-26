using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using Zenject;

public class EnemyInputSystem : IActionFiller {
    public UniTask<T> ProcessRequirementAsync<T>(Opponent requestingPlayer, IRequirement<T> requirement, CancellationToken externalCt = default) where T : class {
        throw new NotImplementedException();
    }
}
