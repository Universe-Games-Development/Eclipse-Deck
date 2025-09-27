using System;
using UnityEngine;

public class UnitView : MonoBehaviour {
    public void Highlight(bool enable) {
        // Реалізація підсвічування
    }
}

public abstract class UnitPresenter {
    public UnitModel Model;
    public UnitView View;

    protected UnitPresenter(UnitModel model, UnitView view) {
        Model = model;
        View = view;
    }

    public void Highlight(bool isEnabled) {
        Debug.Log($"Highlighting unit {Model.GetName()} - {isEnabled}");
    }
}

public class UnitModel {
    public Action<Opponent> OnChangedOwner;
    private Opponent _owner;
    public string Id { get; protected set; }
    public void ChangeOwner(Opponent newOwner) {
        if (newOwner == _owner) return;
        _owner = newOwner;
        OnChangedOwner?.Invoke(newOwner);
    }

    public virtual Opponent GetPlayer() {
        return _owner;
    }

    public virtual string GetName() {
        return "";
    }
}