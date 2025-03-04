using Cysharp.Threading.Tasks;
using System.Threading;

public interface IActionFiller {
    UniTask<T> ProcessRequirementAsync<T>(Opponent requestingPlayer, IRequirement<T> requirement, CancellationToken ct = default) where T : class;
}