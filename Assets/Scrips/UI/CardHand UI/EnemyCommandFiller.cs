using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using Zenject;

public class EnemyCommandFiller : ICardsInputFiller {
    [Inject] IInputRequirementRegistry RequirementRegistry;
    public IInputRequirementRegistry GetRequirementRegistry() {
        return RequirementRegistry;
    }

    public UniTask<T> ProcessRequirementAsync<T>(Opponent cardPlayer, CardInputRequirement<T> requirement) where T : MonoBehaviour {
        throw new NotImplementedException();
    }
}
