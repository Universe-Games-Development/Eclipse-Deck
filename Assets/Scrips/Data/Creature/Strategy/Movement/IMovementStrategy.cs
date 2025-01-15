using Cysharp.Threading.Tasks;

public interface IMoveStrategy {
    public UniTask<int> Movement(GameContext gameContext);
}
