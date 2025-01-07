using UnityEngine;
using Zenject;

[RequireComponent(typeof(FieldSpawner))]
public class GameBoard : MonoBehaviour {
    [SerializeField] private GameObject battleCreaturePrefab;
    [SerializeField] private Canvas tableCanvas;
    private Field[,] fieldGrid;

    [Inject] private DiContainer diContainer;

    private FieldSpawner fieldSpawner;
    [SerializeField] private ObjectDistributer creatureUIDistributor;

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


    public void AssignFieldsToPlayer(Opponent opponent) {
        if (fieldGrid == null) {
            Debug.LogWarning("Field grid has not been generated. Cannot assign fields.");
            return;
        }

        bool isPlayer = opponent is Player;

        string opponentName = !string.IsNullOrEmpty(opponent.Name) ? opponent.Name : "Unnamed Opponent";

        for (int row = 0; row < fieldGrid.GetLength(0); row++) {
            for (int col = 0; col < fieldGrid.GetLength(1); col++) {
                Field currentField = fieldGrid[row, col];
                if (currentField.IsPlayerField == isPlayer) {
                    currentField.AssignOwner(opponent);
                }
            }
        }

        Debug.Log($"{(isPlayer ? "Player" : "Enemy")} fields have been assigned to {opponentName}.");
    }


    public bool SummonCreature(Opponent player, Card card, Field field) {
        Field gridField = GetFieldFromGrid(field);
        if (gridField == null) {
            Debug.LogError("Поле не знайдено в grid!");
            return false;
        }

        if (gridField.Owner == player) {
            if (gridField.OccupiedCreature != null) {
                Debug.Log($"{name} вже зайняте");
                return false; // Поле вже зайняте
            }

            GameObject battleCreatureObj = diContainer.InstantiatePrefab(battleCreaturePrefab, gridField.spawnPoint);
            BattleCreature battleCreature = battleCreatureObj.GetComponent<BattleCreature>();
            battleCreature.Initialize(card, new SingleAttack(), field);

            GameObject creatureUIObj = creatureUIDistributor.CreateObject();
            CreatureUI creatureUI = creatureUIObj.GetComponent<CreatureUI>();
            creatureUI.PositionPanelInWorld(field.uiPoint);

            gridField.AssignCreature(battleCreature);
            return true;
        } else {
            Debug.LogWarning("Це поле належить іншому гравцеві! Не можна зіграти карту тут.");
            return false;
        }
    }



    private Field GetFieldFromGrid(Field field) {
        for (int row = 0; row < fieldGrid.GetLength(0); row++) {
            for (int col = 0; col < fieldGrid.GetLength(1); col++) {
                if (fieldGrid[row, col] == field) {
                    return fieldGrid[row, col];
                }
            }
        }
        return null;
    }
}
