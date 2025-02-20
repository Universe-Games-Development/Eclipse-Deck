using Cysharp.Threading.Tasks;

public interface IAbilityInputter {
    UniTask<T> ProcessRequirementAsync<T>(Opponent requestingPlayer, IRequirement<T> requirement) where T : class;
}