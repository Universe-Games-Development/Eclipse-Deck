using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class GameBoardPresenter : MonoBehaviour {
    [SerializeField] private CreatureSpawner creatureSpawner;
    [SerializeField] private BoardView boardView;
    // Scene Context
    [Inject] private GameboardBuilder _boardManager;

    [Header("Grid Interaction Params")]
    [Range(0, 10)]
    public float yInteractionRange = 1f;

    private OpponentRegistrator OpponentRegistrator;
    [SerializeField] private GameBoardHealthSystem healthSystem;

    [Inject]
    public void Construct(OpponentRegistrator opponentRegistrator) {
        OpponentRegistrator = opponentRegistrator;
        OpponentRegistrator.OnOpponentsRegistered += PrepareBoardBattle;
    }

    private void PrepareBoardBattle(List<Opponent> oppponents) {
        healthSystem.AssignOpponents(oppponents);
        _boardManager.BuildNewBoard();
    }

    public bool IsValidFieldSelected(Field field) {
        if (!IsInitialized()) {
            Debug.LogWarning("Gameboard not initialized! Can't select field");
            return false;
        }

        if (!_boardManager.GridBoard.FieldExists(field)) {
            return false;
        }

        return true;
    }

    public bool IsInitialized() {
        if (_boardManager.GridBoard == null || _boardManager.GridBoard.Config == null) {
            Debug.LogWarning("GridManager is not properly initialized: Global grid is null or empty.");
            return false;
        }

        return true;
    }
    public FieldPresenter GetFieldController(Field targetField) {
        return boardView.GetController(targetField);
    }

    public bool TryGetField(out Field field, Vector3? mouseWorldPosition) {
        field = null;
        if (_boardManager == null || _boardManager.GridBoard == null) {
            return false;
        }
        if (!mouseWorldPosition.HasValue || boardView == null) {
            return false;
        }

        Transform origin = boardView.GetBoardOrigin();
        Vector3 worldPosition = mouseWorldPosition.Value;

        // Check if the click is within the y range
        if (Mathf.Abs(worldPosition.y - origin.position.y) > yInteractionRange) {
            return false;
        }

        // Get grid index by world position
        Vector2Int? gridIndex = _boardManager.GridBoard.GetGridIndexByWorld(origin, worldPosition);
        if (!gridIndex.HasValue) {
            return false;
        }

        // ѕолучаем поле по индексам
        field = _boardManager.GridBoard.GetFieldAt(gridIndex.Value.x, gridIndex.Value.y);

        // ≈сли поле не может быть выбрано Ц выводим предупреждение и выходим
        if (field == null || !IsValidFieldSelected(field)) {
            if (field != null) {
                Debug.LogWarning($"Can't select field: {field.GetTextCoordinates()}");
            }
            return false;
        }

        //Debug.Log($"Selected: {field.GetTextCoordinates()}");
        return true;
    }

    public void OnDestroy() {
        if (OpponentRegistrator != null) {
            OpponentRegistrator.OnOpponentsRegistered -= PrepareBoardBattle;
        }
    }

    public async UniTask<bool> SpawnCreature(CreatureCard creatureCard, Field field, Opponent summoner) {
        return await creatureSpawner.SpawnCreature(creatureCard, field, summoner);
    }
}

