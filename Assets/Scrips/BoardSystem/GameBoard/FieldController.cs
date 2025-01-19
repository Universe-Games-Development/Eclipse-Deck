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

    [SerializeField] private Material attackMaterial;
    [SerializeField] private Material supportMaterial;
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private Levitator levitator;

    bool isInteractable = false;

    private void Awake() {
        meshRenderer = GetComponentInChildren<MeshRenderer>();
        levitator = GetComponentInChildren<Levitator>();
        levitator.UpdateInitialPosition(transform.position);
        levitator.OnFall += () => SetInteractable(true);
        levitator.FlyToInitialPosition();
    }

    public void Initialize(Field field) {
        this.field = field;
        UpdateMaterialBasedOnType(field.Type);
        field.OnChangedType += UpdateMaterialBasedOnType;
        field.OnRemoval += Remove;
    }

    private void Remove(Field field) {
        if (field != this.field) {
            Debug.LogWarning("Trying to remove by wrong field");
        }
        SetInteractable(false);
        field.OnChangedType -= UpdateMaterialBasedOnType;
        field.OnRemoval -= Remove;
        levitator.FlyAway();
    }

    private void SetInteractable(bool value) {
        isInteractable = value;
        if (value) {
            field.OnSelect += levitator.ToggleLevitation;
        } else {
            field.OnSelect -= levitator.ToggleLevitation;
        }
    }

    private void UpdateMaterialBasedOnType(FieldType newType) {
        if (meshRenderer != null) {
            if (newType == FieldType.Support) {
                meshRenderer.material = supportMaterial;
            } else {
                meshRenderer.material = attackMaterial;
            }
        }
    }

    void OnMouseEnter() {
        uiManager.ShowTip("Field");
    }

    private void OnDestroy() {
        field.OnChangedType -= UpdateMaterialBasedOnType;
        field.OnSelect -= levitator.ToggleLevitation;
    }
}
