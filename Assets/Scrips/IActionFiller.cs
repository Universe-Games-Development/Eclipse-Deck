using Cysharp.Threading.Tasks;

public interface IActionFiller {
    UniTask<T> ProcessRequirementAsync<T>(Opponent requestingPlayer, IRequirement<T> requirement) where T : class;
}