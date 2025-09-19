using UnityEngine;
using Zenject;

public abstract class UnitPresenter : MonoBehaviour {
    [Inject] public IUnitPresenterRegistry _unitRegistry;

    [SerializeField] private bool isDebugEnabled = false;
    public bool IsDebugEnabled => isDebugEnabled;

    public abstract UnitModel GetModel();

    protected void DebugLog(string message) {
        if (isDebugEnabled) {
            Debug.Log($"[DEBUG] {GetType().Name}: {message}", this);
        }
    }

    public virtual void Highlight(bool enable) {
        //DebugLog($"Highlight {(enable ? "enabled" : "disabled")}");
    }

    private void Awake() {
        _unitRegistry?.Register(GetModel(), this);
    }

    protected virtual void OnDestroy() {
        UnRegisterOutGame();
    }

    protected void RegisterInGame() {
        _unitRegistry?.Register(GetModel(), this);
    }

    protected void UnRegisterOutGame() {
        _unitRegistry?.Unregister(this);
    }
}
