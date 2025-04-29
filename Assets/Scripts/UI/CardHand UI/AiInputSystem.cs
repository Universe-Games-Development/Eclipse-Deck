using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class AiInputSystem : ITargetingService {
    public List<object> FindValidTargets(IRequirement value, IGameContext context) {
        throw new NotImplementedException();
    }

    public UniTask<object> ProcessRequirementAsync(BoardPlayer requestOpponent, IRequirement requirement) {
        throw new NotImplementedException();
    }
}
