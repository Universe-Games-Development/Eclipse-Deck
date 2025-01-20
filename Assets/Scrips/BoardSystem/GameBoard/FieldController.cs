using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using Zenject;

public enum FieldType {
    Support,
    Attack
}

public class FieldController : MonoBehaviour {

    private Field field;
    [SerializeField] public FieldUI fieldUI;

    [Inject] UIManager uiManager;


    private FieldMaterializer fieldMaterializer;
    private Levitator levitator;

    bool isInteractable = false;

    private void Awake() {
        fieldMaterializer = GetComponentInChildren<FieldMaterializer>();
        levitator = GetComponentInChildren<Levitator>();
        levitator.UpdateInitialPosition(transform.position);
        levitator.OnFall += () => SetInteractable(true);
        levitator.FlyToInitialPosition();
    }

    public void Initialize(Field field) {
        fieldMaterializer.Initialize(field);

        this.field = field;
        field.OnRemoval += Remove;
    }

    private void Remove(Field field) {
        if (field != this.field) {
            Debug.LogWarning("Trying to remove by wrong field");
        }
        SetInteractable(false);
        field.OnRemoval -= Remove;
        levitator.FlyAway();
    }

    private void SetInteractable(bool value) {
        isInteractable = value;
        if (field == null) return;
        if (value) {
            field.OnSelect += levitator.ToggleLevitation;
            field.OnSelect += fieldMaterializer.ToggleHighlight;
        } else {
            field.OnSelect -= levitator.ToggleLevitation;
            field.OnSelect -= fieldMaterializer.ToggleHighlight;
        }
    }

    

    void OnMouseEnter() {
        uiManager.ShowTip("Field");
    }

    private void OnDestroy() {
        field.OnSelect -= levitator.ToggleLevitation;
    }
}
