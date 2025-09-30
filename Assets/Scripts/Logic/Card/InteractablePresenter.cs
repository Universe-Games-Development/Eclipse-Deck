using System;
using Zenject;

public abstract class InteractablePresenter : UnitPresenter, IDisposable {
    [Inject] private readonly IEventBus<IEvent> _eventBus;
    public bool IsInteractable { get; private set; }
    InteractableView view;
    protected InteractablePresenter(UnitModel model, InteractableView view)
        : base(model, view) {
        view.OnClicked += OnClicked;
        view.OnHoverChanged += OnHoverChanged;
        SetInteractable(true);
    }

    private void OnHoverChanged(UnitView view, bool isHovered) {
        if (IsInteractable)
            _eventBus.Raise(new HoverUnitEvent(this, isHovered));
    }

    private void OnClicked(UnitView view) {
        if (IsInteractable)
            _eventBus.Raise(new ClickUnitEvent(this));
    }

    public void SetInteractable(bool isEnabled) {
        IsInteractable = isEnabled;
    }

    public virtual void Dispose() {
        if (view == null) return;
        view.OnClicked -= OnClicked;
        view.OnHoverChanged -= OnHoverChanged;
    }
}

public readonly struct HoverUnitEvent : IEvent {
    public UnitPresenter UnitPresenter { get; }
    public bool IsHovered { get; }

    public HoverUnitEvent(UnitPresenter unitPresenter, bool isHovered) {
        UnitPresenter = unitPresenter;
        IsHovered = isHovered;
    }
}

public readonly struct ClickUnitEvent : IEvent {
    public UnitPresenter UnitPresenter { get; }

    public ClickUnitEvent(UnitPresenter unitPresenter) {
        UnitPresenter = unitPresenter;
    }
}