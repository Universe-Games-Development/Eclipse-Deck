using UnityEngine;
using Zenject;

public class CreatureSummoner : MonoBehaviour {
    [SerializeField] private ObjectDistributer battleCreatureDistributor;
    [SerializeField] private ObjectDistributer creatureUIDistributor;

    public bool SummonCreature(Opponent player, Card card, Field field, FieldManager fieldManager) {
        Field gridField = fieldManager.ValidateSummon(player, field);
        if (gridField == null) return false;

        if (!TryCreateBattleCreature(card, gridField, out var battleCreature)) return false;
        CreateCreatureUI(card, gridField);

        gridField.AssignCreature(battleCreature);
        return true;
    }

    private bool TryCreateBattleCreature(Card card, Field field, out BattleCreature battleCreature) {
        GameObject battleCreatureObj = battleCreatureDistributor.CreateObject();
        battleCreatureObj.transform.position = field.spawnPoint.position;

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
        creatureUI.Initialize(card);
        creatureUI.PositionPanelInWorld(field.uiPoint);
    }
}
