using System;
using UnityEngine;

public abstract class UnitView : MonoBehaviour {

    public virtual void Highlight(bool enable) {
        // Базова реалізація підсвічування
    }
}

public abstract class UnitPresenter {
    public UnitModel Model { get; }
    public UnitView View { get; }
   

    protected UnitPresenter(UnitModel model, UnitView view) {
        Model = model;
        View = view;
    }


    public virtual void Highlight(bool isEnabled) {
        View.Highlight(isEnabled);
    }
}


public class UnitModel {
    public string Id;
    public virtual string OwnerId { get; protected set; }
    public Action<string> OnChangedOwner;

    public string UnitName { get; protected set; }

    public void ChangeOwner(string newOwnerId) {
        if (string.IsNullOrEmpty(newOwnerId) || newOwnerId == OwnerId) return;
        OwnerId = newOwnerId;
        OnChangedOwner?.Invoke(newOwnerId);
    }

    public virtual string GetName() {
        return string.IsNullOrEmpty(UnitName) ? ToString() : UnitName;
    }
}

