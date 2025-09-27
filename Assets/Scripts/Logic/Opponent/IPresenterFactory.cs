using Zenject;

public interface IPresenterFactory {
    TPresenter CreatePresenter<TPresenter>(params object[] args)
        where TPresenter : class;
    TPresenter CreateUnitPresenter<TPresenter>(params object[] args)
        where TPresenter : UnitPresenter;
    void Unregister(UnitPresenter presenter);
}

public class PresenterFactory : IPresenterFactory {
    [Inject] private DiContainer _container;
    [Inject] private IUnitRegistry _unitRegistry;

    // Загальний метод для всіх презентерів
    public TPresenter CreatePresenter<TPresenter>(params object[] args)
        where TPresenter : class {
        return _container.Instantiate<TPresenter>(args);
    }

    // Спеціальний метод тільки для UnitPresenter
    public TPresenter CreateUnitPresenter<TPresenter>(params object[] args)
        where TPresenter : UnitPresenter {
        TPresenter presenter = _container.Instantiate<TPresenter>(args);
        _unitRegistry.Register(presenter);
        return presenter;
    }

    public void Unregister(UnitPresenter presenter) {
        _unitRegistry.Unregister(presenter);
    }
}