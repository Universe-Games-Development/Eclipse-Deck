using System.Collections.Generic;
using UnityEngine;
using Zenject;

[RequireComponent(typeof(FieldSpawner))]
public class GameBoard : MonoBehaviour {
    [SerializeField] private HealthCell playerCell;
    [SerializeField] private HealthCell enemyCell;

    [SerializeField] private GameObject battleCreaturePrefab;
    [SerializeField] private Canvas tableCanvas;
    private Field[,] fieldGrid;
    [SerializeField] private ObjectDistributer creatureUIDistributor;
    [Inject] private DiContainer diContainer;

    private FieldSpawner fieldSpawner;

    private void Awake() {
        fieldSpawner = GetComponent<FieldSpawner>();
        if (fieldGrid == null) {
            GenerateGrid();
        }
    }

    private void GenerateGrid() {
        fieldSpawner.GenerateGrid();
        fieldGrid = fieldSpawner.GetFieldGrid();
    }

    #region Game Initialization
    public void AssignOpponent(Opponent opponent) {
        AssignHPCellToOpponent(opponent);
        AssignFieldsToOpponent(opponent);
    }
    private void AssignHPCellToOpponent(Opponent opponent) {
        if (opponent is Player) {
            playerCell.AssignOwner(opponent);
        } else {
            enemyCell.AssignOwner(opponent);
        }
    }

    private void AssignFieldsToOpponent(Opponent opponent) {
        if (fieldGrid == null) {
            Debug.LogWarning("Field grid has not been generated. Cannot assign fields.");
            return;
        }

        foreach (var field in GetFieldsForOpponent(opponent)) {
            field.AssignOwner(opponent);
        }

        Debug.Log($"{(opponent is Player ? "Player" : "Enemy")} fields have been assigned to {opponent.Name ?? "Unnamed Opponent"}.");
    }

    private IEnumerable<Field> GetFieldsForOpponent(Opponent opponent) {
        bool isPlayer = opponent is Player;
        foreach (var field in fieldGrid) {
            if (field.IsPlayerField == isPlayer) {
                yield return field;
            }
        }
    }

    #endregion

    #region Summoning
    public bool SummonCreature(Opponent player, Card card, Field field) {
        Field gridField = ValidateField(player, field);
        if (gridField == null) return false;

        if (!TryCreateBattleCreature(card, gridField, out var battleCreature)) return false;
        CreateCreatureUI(card, gridField);

        gridField.AssignCreature(battleCreature);
        return true;
    }

    private Field ValidateField(Opponent player, Field field) {
        if (field == null) {
            Debug.LogError("Поле не знайдено в grid!");
            return null;
        }

        if (field.Owner != player) {
            Debug.LogWarning("Це поле належить іншому гравцеві! Не можна зіграти карту тут.");
            return null;
        }

        if (field.OccupiedCreature != null) {
            Debug.Log($"{field.name} вже зайняте");
            return null;
        }

        return field;
    }

    private bool TryCreateBattleCreature(Card card, Field field, out BattleCreature battleCreature) {
        battleCreature = null;
        if (battleCreaturePrefab == null) {
            Debug.LogError("Battle creature prefab is not set!");
            return false;
        }

        GameObject battleCreatureObj = diContainer.InstantiatePrefab(battleCreaturePrefab, field.spawnPoint);
        battleCreature = battleCreatureObj.GetComponent<BattleCreature>();
        if (battleCreature == null) {
            Debug.LogError("Failed to create BattleCreature.");
            Destroy(battleCreatureObj);
            return false;
        }

        battleCreature.Initialize(card, new SingleAttack(), field);
        return true;
    }

    private void CreateCreatureUI(Card card, Field field) {
        GameObject creatureUIObj = creatureUIDistributor.CreateObject();
        CreatureUI creatureUI = creatureUIObj.GetComponent<CreatureUI>();
        creatureUI.Initialize(creatureUIDistributor, card);
        creatureUI.PositionPanelInWorld(field.uiPoint);
    }

    #endregion
}
