using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Реєстр для зв'язування Presenter View Model
/// </summary>
public interface IUnitRegistry {
    // Основна реєстрація - тільки презентер
    void Register(UnitPresenter presenter);
    void Unregister(UnitPresenter presenter);
    void UnregisterByModel(UnitModel model);
    void UnregisterByView(UnitView view);

    // Отримання презентера
    TPresenter GetPresenter<TPresenter>() where TPresenter : UnitPresenter;
    TPresenter GetPresenter<TPresenter>(UnitModel model) where TPresenter : UnitPresenter;
    UnitPresenter GetPresenterByModel(UnitModel model);
    TPresenter GetPresenter<TPresenter>(UnitView view) where TPresenter : UnitPresenter;
    UnitPresenter GetPresenterByView(UnitView view);

    // Отримання через презентер (оскільки він містить Model та View)
    TModel GetModel<TModel>(UnitPresenter presenter) where TModel : UnitModel;
    TView GetView<TView>(UnitPresenter presenter) where TView : UnitView;

    // Пряме отримання Model/View (найчастіше використовувані)
    TView GetViewByModel<TView>(UnitModel model) where TView : UnitView;
    UnitView GetViewByModel(UnitModel model);
    TModel GetModelByView<TModel>(UnitView view) where TModel : UnitModel;
    UnitModel GetModelByView(UnitView view);

    // Try-методи
    bool TryGetPresenterByModel<TPresenter>(UnitModel model, out TPresenter presenter) where TPresenter : UnitPresenter;
    bool TryGetPresenterByView<TPresenter>(UnitView view, out TPresenter presenter) where TPresenter : UnitPresenter;
    bool TryGetViewByModel<TView>(UnitModel model, out TView view) where TView : UnitView;
    bool TryGetModelByView<TModel>(UnitView view, out TModel model) where TModel : UnitModel;

    // Утилітарні методи
    IEnumerable<TPresenter> GetAllPresenters<TPresenter>() where TPresenter : UnitPresenter;
    IReadOnlyCollection<UnitPresenter> GetAllPresenters();
    IEnumerable<TModel> GetAllModels<TModel>() where TModel : UnitModel;
    IEnumerable<TView> GetAllViews<TView>() where TView : UnitView;

    bool IsRegistered(UnitPresenter presenter);
    bool IsRegisteredByModel(UnitModel model);
    bool IsRegisteredByView(UnitView view);

    void Clear();
}

public class UnitRegistry : IUnitRegistry, IDisposable {
    // Один основний список презентерів
    private readonly HashSet<UnitPresenter> _presenters = new();

    // Індекси для швидкого пошуку
    private readonly Dictionary<UnitModel, UnitPresenter> _modelToPresenter = new();
    private readonly Dictionary<UnitView, UnitPresenter> _viewToPresenter = new();

    #region Registration Methods

    public void Register(UnitPresenter presenter) {
        if (presenter == null) {
            Debug.LogError("Presenter cannot be null");
            return;
        }

        // Очищуємо конфлікти
        UnregisterByModel(presenter.Model);
        UnregisterByView(presenter.View);

        // Додаємо презентер
        _presenters.Add(presenter);
        _modelToPresenter[presenter.Model] = presenter;
        _viewToPresenter[presenter.View] = presenter;

        Debug.Log($"Registered {presenter.GetType().Name}: {presenter.Model}");
    }

    public void Unregister(UnitPresenter presenter) {
        if (presenter == null) return;
        UnregisterInternal(presenter);
    }

    public void UnregisterByModel(UnitModel model) {
        if (model == null) return;

        if (_modelToPresenter.TryGetValue(model, out var presenter)) {
            UnregisterInternal(presenter);
        }
    }

    public void UnregisterByView(UnitView view) {
        if (view == null) return;

        if (_viewToPresenter.TryGetValue(view, out var presenter)) {
            UnregisterInternal(presenter);
        }
    }

    #endregion

    #region Presenter Getters

    public TPresenter GetPresenter<TPresenter>() where TPresenter : UnitPresenter {
        return _presenters.OfType<TPresenter>().FirstOrDefault();
    }

    public TPresenter GetPresenter<TPresenter>(UnitModel model) where TPresenter : UnitPresenter {
        if (model == null) return null;

        return _modelToPresenter.GetValueOrDefault(model) as TPresenter;
    }

    public UnitPresenter GetPresenterByModel(UnitModel model) {
        if (model == null) return null;

        return _modelToPresenter.GetValueOrDefault(model);
    }

    public TPresenter GetPresenter<TPresenter>(UnitView view) where TPresenter : UnitPresenter {
        if (view == null) return null;

        return _viewToPresenter.GetValueOrDefault(view) as TPresenter;
    }

    public UnitPresenter GetPresenterByView(UnitView view) {
        if (view == null) return null;

        return _viewToPresenter.GetValueOrDefault(view);
    }

    #endregion

    #region Model/View through Presenter

    public TModel GetModel<TModel>(UnitPresenter presenter) where TModel : UnitModel {
        return presenter?.Model as TModel;
    }

    public TView GetView<TView>(UnitPresenter presenter) where TView : UnitView {
        return presenter?.View as TView;
    }

    #endregion

    #region Direct Model/View Access

    public TView GetViewByModel<TView>(UnitModel model) where TView : UnitView {
        var presenter = GetPresenterByModel(model);
        return presenter?.View as TView;
    }

    public UnitView GetViewByModel(UnitModel model) {
        var presenter = GetPresenterByModel(model);
        return presenter?.View;
    }

    public TModel GetModelByView<TModel>(UnitView view) where TModel : UnitModel {
        var presenter = GetPresenterByView(view);
        return presenter?.Model as TModel;
    }

    public UnitModel GetModelByView(UnitView view) {
        var presenter = GetPresenterByView(view);
        return presenter?.Model;
    }

    #endregion

    #region Try Methods

    public bool TryGetPresenterByModel<TPresenter>(UnitModel model, out TPresenter presenter) where TPresenter : UnitPresenter {
        presenter = GetPresenter<TPresenter>(model);
        return presenter != null;
    }

    public bool TryGetPresenterByView<TPresenter>(UnitView view, out TPresenter presenter) where TPresenter : UnitPresenter {
        presenter = GetPresenter<TPresenter>(view);
        return presenter != null;
    }

    public bool TryGetViewByModel<TView>(UnitModel model, out TView view) where TView : UnitView {
        view = GetViewByModel<TView>(model);
        return view != null;
    }

    public bool TryGetModelByView<TModel>(UnitView view, out TModel model) where TModel : UnitModel {
        model = GetModelByView<TModel>(view);
        return model != null;
    }

    #endregion

    #region Utility Methods

    public IEnumerable<TPresenter> GetAllPresenters<TPresenter>() where TPresenter : UnitPresenter {
        return _presenters.OfType<TPresenter>();
    }

    public IReadOnlyCollection<UnitPresenter> GetAllPresenters() {
        // Це все ще повертає копію, але тип інтерфейсу більш чіткий
        return _presenters.ToList().AsReadOnly();
    }

    public IEnumerable<TModel> GetAllModels<TModel>() where TModel : UnitModel {
        return _presenters.Select(p => p.Model).OfType<TModel>();
    }

    public IEnumerable<TView> GetAllViews<TView>() where TView : UnitView {
        return _presenters.Select(p => p.View).OfType<TView>();
    }

    public bool IsRegistered(UnitPresenter presenter) {
        return presenter != null && _presenters.Contains(presenter);
    }

    public bool IsRegisteredByModel(UnitModel model) {
        return model != null && _modelToPresenter.ContainsKey(model);
    }

    public bool IsRegisteredByView(UnitView view) {
        return view != null && _viewToPresenter.ContainsKey(view);
    }

    public void Clear() {
        _presenters.Clear();
        _modelToPresenter.Clear();
        _viewToPresenter.Clear();
        Debug.Log("Presenter Registry cleared");
    }

    #endregion

    #region Private Methods

    private void UnregisterInternal(UnitPresenter presenter) {
        if (presenter == null) return;

        _presenters.Remove(presenter);
        _modelToPresenter.Remove(presenter.Model);
        _viewToPresenter.Remove(presenter.View);

        Debug.Log($"Unregistered presenter: {presenter}");
    }

    public void Dispose() {
        Clear();
    }

    #endregion
}