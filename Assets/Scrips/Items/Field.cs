using UnityEngine;
using Zenject;

public enum FieldType {
    Support,
    Attack
}

public class Field : MonoBehaviour, ITipProvider {
    private FieldType type;
    public Opponent Owner { get; private set; }
    public bool IsPlayerField;
    public BattleCreature OccupiedCreature { get; private set; }

    public int Index = 0;
    [SerializeField] public Transform spawnPoint;
    [SerializeField] public Transform uiPoint;
    [SerializeField] private MeshRenderer meshRenderer;

    [Inject] UIManager uIManager;

    [SerializeField] private Material defaultMaterial;
    [SerializeField] private Material transparentMaterial;

    private void Awake() {
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null) {
            defaultMaterial = meshRenderer.material;
        }
    }

    // Property Type with logic for setting
    public FieldType Type {
        get => type;
        set {
            type = value;
            UpdateMaterialBasedOnType();
        }
    }

    private void UpdateMaterialBasedOnType() {
        if (meshRenderer != null) {
            if (Type == FieldType.Support) {
                if (transparentMaterial)
                meshRenderer.material = transparentMaterial;
            } else {
                meshRenderer.material = defaultMaterial;
            }
        }
    }

    public bool AssignCreature(BattleCreature creature) {
        if (OccupiedCreature != null) {
            Debug.Log($"{name} вже зайняте");
            return false;
        }
        OccupiedCreature = creature;
        return true;
    }

    public void RemoveCreature() {
        OccupiedCreature = null;
    }

    public void ReceiveAttack(int damage) {
        if (OccupiedCreature != null) {
            OccupiedCreature.card.Health.ApplyDamage(damage);
        } else {
            if (Owner) {
                Owner.health.ApplyDamage(damage);
                Debug.Log($"{Owner.Name} takes {damage} damage, because field {Owner} empty.");
            } else {
                Debug.Log($"Nobody takes {damage} damage");
            }
        }
    }

    public bool IsEmpty() {
        return OccupiedCreature == null;
    }

    // ITipProvider
    public string GetInfo() {
        string info = $"Field #{Index}" +
                      $"\nType: {Type}" +
                      $"\nOwner: {Owner?.Name}";

        if (OccupiedCreature != null) {
            info += $"\n" +
                      $"Creature: {OccupiedCreature.Name} + \n" +
                      $"Hp: {OccupiedCreature.card.Health.CurrentValue} / " +
                      $"Atk: {OccupiedCreature.GetAttack()}";
        } else {
            info += "\nEmpty field.";
        }

        return info;
    }

    [Inject] protected UIManager uiManager;

    void OnMouseEnter() {
        uiManager.ShowTip(this);
    }

    public void AssignOwner(Opponent player1) {
        Owner = player1;
    }

    public void SetFieldOwnerIndicator(Opponent owner) {
    }
}