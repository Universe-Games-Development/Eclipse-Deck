using Cysharp.Threading.Tasks;

public interface IExecutableTask {
    UniTask<bool> Execute();
}