using Zenject;

public interface IVisualTaskFactory {
    TVisualTask Create<TVisualTask>(params object[] args) where TVisualTask : VisualTask;
}

public class VisualTaskFactory : IVisualTaskFactory {
    [Inject] DiContainer container;

    public TVisualTask Create<TVisualTask>(params object[] args) where TVisualTask : VisualTask {
        return container.Instantiate<TVisualTask>(new object[] { args });
    }
}