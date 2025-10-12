using Zenject;

public interface IPresenterFactory {
    TPresenter CreatePresenter<TPresenter>(params object[] args)
        where TPresenter : class;

    TPresenter CreateUnitPresenter<TPresenter>(params object[] args)
        where TPresenter : UnitPresenter;
}

public class PresenterFactory : IPresenterFactory {
    [Inject] private DiContainer _container;
    
    public TPresenter CreatePresenter<TPresenter>(params object[] args)
        where TPresenter : class {
        return _container.Instantiate<TPresenter>(args);
    }
    public TPresenter CreateUnitPresenter<TPresenter>(params object[] args)
        where TPresenter : UnitPresenter {
        TPresenter presenter = _container.Instantiate<TPresenter>(args);

        return presenter;
    }
}