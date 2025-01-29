using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using Zenject;
using System.Collections.Generic;

// Add states soon
public class GameBoard {

    // Opponents will use it to be notified which one can perform turn now
    public Action<Opponent> OnTurnBegan;

    [Inject] GridVisual boardVisual;
    [Inject] public OpponentManager OpponentManager { get; private set; }
    [Inject] private GridManager gridManager;
    [Inject] private CommandManager creatureCommands;

    private Opponent currentPlayer;
    private GameContext gameContext;
    public int MinPlayers { get; private set; }

    private Field _selectedField;

    private Field SelectedField {
        get { return _selectedField; }
        set {
            if (_selectedField != null && _selectedField != value) {
                Debug.Log($"Deselected : {_selectedField.row} + {_selectedField.column}");
                _selectedField.ToggleSelection(false);
            }
            _selectedField = value;
            if (_selectedField != null) {
                _selectedField.ToggleSelection(true);
                Debug.Log($"Selected : {_selectedField.row} + {_selectedField.column}");
            }
        }
    }


    public GameBoard(OpponentManager opponentManager, GridManager gridManager, CommandManager commandManager) {
        gameContext = new GameContext { gameBoard = this, _gridManager = gridManager };
        this.gridManager = gridManager;
        this.OpponentManager = opponentManager;
    }

    public Opponent GetCurrentPlayer() {
        return currentPlayer;
    }

    public void SetBoardSettings(GridSettings boardSettings) {
        gridManager.UpdateGrid(boardSettings);
    }

    // Used by other classes to allow start game
    public async UniTask<bool> StartGame() {
        await UniTask.Delay(50);
        if (!OpponentManager.IsAllRegistered()) {
            Debug.Log($"Can't start game because there are only {OpponentManager.registeredOpponents.Count} registered players. Need: {MinPlayers}");
            return false;
        }

        currentPlayer = ChooseFirstPlayer();
        OnTurnBegan?.Invoke(currentPlayer);
        return true;
    }

    private Opponent ChooseFirstPlayer() {
        // Logic to choose the first player, e.g., randomly
        currentPlayer = OpponentManager.GetRandomOpponent();
        Debug.Log($"{currentPlayer.Name} is chosen to start first.");
        return currentPlayer;
    }

    // Used by any opponent
    public async UniTask PerformTurn(Opponent opponent) {
        if (opponent != currentPlayer) {
            Debug.Log("Not your turn buddy");
            return;
        }
        GatherPlayerCreaturesActions(currentPlayer);
        await creatureCommands.ExecuteCommands();
    }

    private void GatherPlayerCreaturesActions(Opponent opponent) {
        List<List<Field>> opponentBoardPart = OpponentManager.GetOpponentBoard(currentPlayer);

        foreach (var row in opponentBoardPart) {
            if (row == null) {
                Debug.LogWarning("Column with null");
                continue;
            }
            foreach (var field in row) {
                var creature = field.OccupiedCreature;
                if (creature != null) {
                    gameContext.initialField = field;
                    creatureCommands.RegisterCommand(creature.GetTurnActions(gameContext));
                }
            }
            gameContext.initialField = null;
        }

        ChangeTurn();
    }

    private void ChangeTurn() {
        currentPlayer = OpponentManager.GetNextOpponent(currentPlayer);
        Debug.Log($"It is now {currentPlayer.Name}'s turn.");
        OnTurnBegan?.Invoke(currentPlayer);
    }

    public async UniTask<bool> SummonCreature(Opponent opponent, Field field, Creature creature) {
        // Code warnings
        if (field == null || creature == null) {
            Debug.LogWarning("Field or creature is null.");
            return false;
        }

        // Game warnings
        if (currentPlayer != opponent) {
            Debug.LogWarning("It`s not turn of : " + opponent.Name);
        }

        if (opponent != field.Owner) {
            Debug.LogWarning("This field not belong to : " + opponent.Name);
        }


        bool result = await field.PlaceCreatureAsync(creature);
        if (result) {
            Debug.Log($"Creature successfully placed at {field.row}, {field.column}.");
        } else {
            Debug.LogWarning($"Failed to place creature at {field.row}, {field.column}.");
        }

        return result;
    }

    // Can get null to deselect field
    public bool SelectField(Field field) {
        if (!IsInitialized()) {
            Debug.LogWarning("Gameboard not initialized! Can`t select field");
        }

        if (!gridManager.GridBoard.FieldExists(field)) {
            Debug.Log("Field doesn`t exist! Gameboard can`t select : " + field);
            return false;
        }

        // Game rules errors
        if (currentPlayer is Enemy) {
            Debug.Log("Field can`t be selected during enemy turn. Current player : " + currentPlayer);
            return false;
        }

        if (currentPlayer != field.Owner) {
            return false;
        }

        if (SelectedField == field) {
            return true;
        }

        SelectedField = field;
        SelectedField.ToggleSelection(true);
        return true;
    }

    public void DeselectField() {
        if (SelectedField != null) {
            SelectedField.ToggleSelection(false);
            SelectedField = null;
        }
    }


    public bool IsInitialized() {
        if (gridManager.GridBoard == null || gridManager.GridBoard.Config == null) {
            Debug.LogWarning("GridManager is not properly initialized: MainGrid is null or empty.");
            return false;
        }

        if (!OpponentManager.IsAllRegistered()) {
            Debug.LogWarning("Not all players are registered.");
            return false;
        }

        if (currentPlayer == null) {
            Debug.LogWarning("Current player is not set.");
            return false;
        }

        if (gameContext == null || gameContext.gameBoard == null || gameContext._gridManager == null) {
            Debug.LogWarning("Game context is not properly initialized.");
            return false;
        }

        return true;
    }

    public void SetCurrentPlayer(Player player) {
        currentPlayer = player;
    }
}
