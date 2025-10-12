using System;
using Zenject;

public interface IUnitSpawner<TModel, TView, TPresenter>
    where TModel : UnitModel
    where TView : UnitView
    where TPresenter : UnitPresenter {
    TPresenter SpawnUnit(TModel model, bool registerInSystems = true);
    void RemoveUnit(TPresenter presenter);
}

public class UnitSpawner<TModel, TView, TPresenter> : IUnitSpawner<TModel, TView, TPresenter>
    where TModel : UnitModel
    where TView : UnitView
    where TPresenter : UnitPresenter {
    [Inject] private IPresenterFactory _presenterFactory;
    [Inject] private IComponentPool<TView> _viewPool;
    [Inject] private IUnitRegistry _unitRegistry;

    public TPresenter SpawnUnit(TModel model, bool needRegistration = true) {
        var view = _viewPool.Get();
        if (view == null) throw new InvalidOperationException($"No available {typeof(TView).Name} in pool");

        var presenter = _presenterFactory.CreateUnitPresenter<TPresenter>(model, view);
        view.gameObject.name = $"Creature_{model}";

        if (needRegistration)
        _unitRegistry.Register(presenter);
        return presenter;
    }

    public void RemoveUnit(TPresenter presenter) {
        if (presenter?.View is TView view) {
            _unitRegistry.Unregister(presenter);
            _viewPool.Release(view);

            if (presenter is IDisposable disposable) {
                disposable.Dispose();
            }
        }
    }
}
