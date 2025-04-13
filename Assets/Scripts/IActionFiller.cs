using Cysharp.Threading.Tasks;

public interface IActionFiller {
    UniTask<object> ProcessRequirementAsync(IRequirement requirement);
    //UniTask<T> ProcessRequirementAsync<T>(IRequirement requirement, CancellationToken externalCt = default) where T : class;
}