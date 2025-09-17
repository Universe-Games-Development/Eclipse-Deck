using System.Collections.Generic;

public interface IUnitPresenterRegistry {
    void Register(UnitModel model, UnitPresenter presenter);
    void Unregister(UnitModel model);
    void Unregister(UnitPresenter model);
    T GetPresenter<T>(UnitModel model) where T : UnitPresenter;
    UnitModel GetModel(UnitPresenter presenter);
    IEnumerable<UnitPresenter> GetAllPresenters();
}

public class UnitPresenterRegistry : IUnitPresenterRegistry {
    private readonly Dictionary<UnitModel, UnitPresenter> _modelToPresenter = new();
    private readonly Dictionary<UnitPresenter, UnitModel> _presenterToModel = new();

    public void Register(UnitModel model, UnitPresenter presenter) {
        if (model == null || presenter == null) return;

        Unregister(model);
        Unregister(presenter);

        _modelToPresenter[model] = presenter;
        _presenterToModel[presenter] = model;
    }

    public void Unregister(UnitModel model) {
        if (model != null && _modelToPresenter.Remove(model, out var presenter)) {
            _presenterToModel.Remove(presenter);
        }
    }

    public void Unregister(UnitPresenter presenter) {
        if (presenter != null && _presenterToModel.Remove(presenter, out var model)) {
            _modelToPresenter.Remove(model);
        }
    }

    public T GetPresenter<T>(UnitModel model) where T : UnitPresenter {
        return _modelToPresenter.TryGetValue(model, out var presenter) ? presenter as T : null;
    }

    public UnitModel GetModel(UnitPresenter presenter) {
        return _presenterToModel.TryGetValue(presenter, out var model) ? model : null;
    }

    public IEnumerable<UnitPresenter> GetAllPresenters() {
        return _presenterToModel.Keys;
    }
}