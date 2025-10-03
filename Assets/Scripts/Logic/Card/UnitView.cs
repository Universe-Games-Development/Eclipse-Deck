using System;
using Unity.VisualScripting;
using UnityEngine;

public abstract class UnitView : MonoBehaviour {

    public virtual void Highlight(bool enable) {
        // Базова реалізація підсвічування
    }
}

public class LayoutView : MonoBehaviour {
    ILayout3DHandler layout;

    [SerializeField] public LayoutSettings settings;
    [SerializeField] Transform itemsContainer;

    public event Action OnUpdateRequest;
    [SerializeField] float updateTime = 1f;
    [SerializeField] bool doUpdate = false;
    private float lastTime;

    protected void Awake() {
        layout = new Grid3DLayout(settings);
    }

    private void Update() {
        if (!doUpdate) return;
        lastTime += Time.deltaTime;
        if (lastTime > updateTime) {
            OnUpdateRequest?.Invoke();
            lastTime = 0;
        }
    }

    public void UpdatePositions() {

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

    public void ChangeOwner(string newOwnerId) {
        if (string.IsNullOrEmpty(newOwnerId) || newOwnerId == OwnerId) return;
        OwnerId = newOwnerId;
        OnChangedOwner?.Invoke(newOwnerId);
    }

    public virtual string GetName() {
        return this.ToString();
    }
}

