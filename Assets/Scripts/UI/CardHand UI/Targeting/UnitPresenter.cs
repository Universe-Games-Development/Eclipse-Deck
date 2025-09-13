using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public abstract class UnitPresenter : MonoBehaviour {
    [Inject] public IUnitRegistry _unitRegistry;

    [SerializeField] private bool isDebugEnabled = false;
    public bool IsDebugEnabled => isDebugEnabled;

    public abstract UnitModel GetInfo();
    public abstract BoardPlayer GetPlayer();

    protected void DebugLog(string message) {
        if (isDebugEnabled) {
            Debug.Log($"[DEBUG] {GetType().Name}: {message}", this);
        }
    }

    public virtual void Highlight(bool enable) {
        DebugLog($"Highlight {(enable ? "enabled" : "disabled")}");
    }

    protected virtual void OnEnable() {
        _unitRegistry?.Register(this);
    }

    protected virtual void OnDisable() {
        _unitRegistry?.Unregister(this);
    }
}

public interface IUnitRegistry {
    bool Register(UnitPresenter presenter);
    bool Unregister(UnitPresenter presenter);
    bool Contains(UnitPresenter presenter);
    IEnumerable<UnitPresenter> GetAllUnits();
}

public class UnitRegistry : IUnitRegistry {
    private readonly HashSet<UnitPresenter> registeredUnits = new();

    /// <summary>
    /// Реєструє юніта у реєстрі.
    /// </summary>
    public bool Register(UnitPresenter presenter) {
        if (presenter == null)
            throw new ArgumentNullException(nameof(presenter));

        // HashSet сам по собі не дає дублікатів,
        // але повертає true/false для успішності
        return registeredUnits.Add(presenter);
    }

    /// <summary>
    /// Видаляє юніта з реєстру.
    /// </summary>
    public bool Unregister(UnitPresenter presenter) {
        if (presenter == null)
            throw new ArgumentNullException(nameof(presenter));

        return registeredUnits.Remove(presenter);
    }

    /// <summary>
    /// Перевіряє чи є юніт у реєстрі.
    /// </summary>
    public bool Contains(UnitPresenter presenter) {
        if (presenter == null) return false;
        return registeredUnits.Contains(presenter);
    }

    /// <summary>
    /// Отримати всі зареєстровані юніти.
    /// </summary>
    public IEnumerable<UnitPresenter> GetAllUnits() {
        // Оскільки у нас HashSet, null не може бути доданий, 
        // але можна залишити без Where
        return registeredUnits;
    }
}

