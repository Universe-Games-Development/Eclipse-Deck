using System;
using Zenject;

public interface ICreatureFactory {
    Creature CreateModel(CreatureCard card);
}

public class CreatureFactory : ICreatureFactory{

    [Inject] private DiContainer _container;

    public Creature CreateModel(CreatureCard card) {
        return _container.Instantiate<Creature>(new object[] { card });
    }
}

public interface IUnitSpawner<TModel, TView, TPresenter>
    where TModel : UnitModel
    where TView : UnitView
    where TPresenter : UnitPresenter {
    TPresenter SpawnUnit(TModel model);
    void RemoveUnit(TPresenter presenter);
}

public class UnitSpawner<TModel, TView, TPresenter> : IUnitSpawner<TModel, TView, TPresenter>
    where TModel : UnitModel
    where TView : UnitView
    where TPresenter : UnitPresenter {
    [Inject] private IPresenterFactory _presenterFactory;
    [Inject] private IComponentPool<TView> _viewPool;

    public TPresenter SpawnUnit(TModel model) {
        var view = _viewPool.Get();
        if (view == null) throw new InvalidOperationException($"No available {typeof(TView).Name} in pool");

        var presenter = _presenterFactory.CreateUnitPresenter<TPresenter>(model, view);

        return presenter;
    }

    public void RemoveUnit(TPresenter presenter) {
        if (presenter?.View is TView view) {
            _presenterFactory.Unregister(presenter);
            _viewPool.Release(view);

            if (presenter is IDisposable disposable) {
                disposable.Dispose();
            }
        }
    }
}
