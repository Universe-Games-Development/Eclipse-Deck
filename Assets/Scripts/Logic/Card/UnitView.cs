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
    public string Id;
    public virtual string OwnerId { get; protected set; }
    public Action<string> OnChangedOwner;

    public void ChangeOwner(string newOwnerId) {
        if (string.IsNullOrEmpty(newOwnerId) || newOwnerId == OwnerId) return;
        OwnerId = newOwnerId;
        OnChangedOwner?.Invoke(newOwnerId);
    }

    public virtual string GetName() {
        return this.ToString();
    }
}