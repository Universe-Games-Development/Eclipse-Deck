using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using System.Collections.Generic;
using Zenject;

public class GameBoard {
    // Zenject
    private BoardUpdater _boardUpdater;
    private TurnManager _turnManager;

    public GameBoard(BoardUpdater boardUpdater, TurnManager turnManager) {
        _boardUpdater = boardUpdater;
        _turnManager = turnManager;
    }

    public bool SummonCreature(Opponent opponent, Field field, Creature creature) {
        if (!IsValidSummon(opponent, field, creature)) return false;

        bool result = field.PlaceCreature(creature);
        Debug.Log(result
            ? $"Creature successfully placed at {field.GetTextCoordinates()}"
            : $"Failed to place creature at {field.row}, {field.column}.");
        return result;
    }

    private bool IsValidSummon(Opponent opponent, Field field, Creature creature) {
        if (field == null || creature == null) {
            Debug.LogWarning("Invalid summon attempt: Field or creature is null.");
            return false;
        }

        if (opponent != field.Owner) {
            Debug.LogWarning($"{opponent.Name} tried to summon on a field they don't own.");
            return false;
        }
        return true;
    }

    public bool IsValidFieldSelected(Field field) {
        if (!IsInitialized()) {
            Debug.LogWarning("Gameboard not initialized! Can't select field");
            return false;
        }

        if (!_boardUpdater.GridBoard.FieldExists(field)) {
            Debug.LogWarning("Field doesn’t exist! Gameboard can’t select: " + field);
            return false;
        }

        if (_turnManager.ActiveOpponent != field.Owner) {
            Debug.LogWarning("Field does not belong to the current player.");
            return false;
        }

        return true;
    }

    public bool IsInitialized() {
        if (_boardUpdater.GridBoard == null || _boardUpdater.GridBoard.Config == null) {
            Debug.LogWarning("GridManager is not properly initialized: Global grid is null or empty.");
            return false;
        }

        return true;
    }
}


// This class will handle all actions: Creatures abilities, moves, spells, opponent end turn and other battle actions, Even Dialogues!
public class BatttleActionManager {
    private TurnManager TurnManager;
    private GameContext _gameContext;

    // Zenject
    private CommandManager _battleActionCommander;

    private GameBoard _gameBoard;
    private BoardAssigner _boardAssigner;

    [Inject]
    public void Construct(CommandManager battleActionCommander, GameBoard gameBoard, BoardAssigner boardAssigner) {
        _gameBoard = gameBoard;
        _battleActionCommander = battleActionCommander;
        _boardAssigner = boardAssigner; // ²í'ºêö³ÿ BoardAssigner
    }

    public async UniTask PerformCreatureActions(Opponent opponent) {
        try {
            foreach (var creature in _boardAssigner.GetOpponentCreatures(TurnManager.ActiveOpponent)) {
                _battleActionCommander.RegisterCommand(creature.GetTurnActions(_gameContext));
            }

            await _battleActionCommander.ExecuteCommands();
        } catch (Exception ex) {
            Debug.LogError($"Error while performing creature actions for opponent {opponent.Name}: {ex.Message}");
        }
    }
}

public class BattleManager {
    public Action OnBattleStarted;
    public Action OnBattleFinished;

    [Inject] private OpponentRegistrator Registrator;
    [Inject] private BoardUpdater _boardUpdater;

    [Inject]
    private void Construct(OpponentRegistrator registrator) {
        Registrator = registrator;
        Registrator.OnOpponentsRegistered += HandleBattleStart;
    }

    public void HandleBattleStart(List<Opponent> opponents) {
        StartBattle().Forget();
    }

    public async UniTask StartBattle() {
        await _boardUpdater.SpawnBoard();
        OnBattleStarted?.Invoke();
    }
}