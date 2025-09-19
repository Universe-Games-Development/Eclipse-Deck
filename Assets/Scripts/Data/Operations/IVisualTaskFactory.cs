using Zenject;

public interface IVisualTaskFactory {
    TVisualTask Create<TVisualTask>(VisualData data) where TVisualTask : VisualTask;
}

public class VisualTaskFactory : IVisualTaskFactory {
    [Inject] DiContainer container;

    public TVisualTask Create<TVisualTask>(VisualData data) where TVisualTask : VisualTask {
        return container.Instantiate<TVisualTask>(new object[] { data });
    }
}