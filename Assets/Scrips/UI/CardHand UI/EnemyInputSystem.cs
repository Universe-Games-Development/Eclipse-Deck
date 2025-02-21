using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using Zenject;

public class EnemyInputSystem : IActionFiller {
    public UniTask<T> ProcessRequirementAsync<T>(Opponent requestingPlayer, IRequirement<T> requirement) where T : class {
        throw new NotImplementedException();
    }
}
