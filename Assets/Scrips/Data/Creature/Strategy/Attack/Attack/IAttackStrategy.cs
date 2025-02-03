using Cysharp.Threading.Tasks;

public interface IAttackStrategy {
    public UniTask<bool> Attack(object data);
}
