using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using Zenject;

public class GameBoard {

    // Opponents will use it to be notified which one can perform turn now
    public Action<Opponent> OnTurnBegan;
    [Inject] public OpponentManager opponentManager { get; private set; }
    [Inject] private GridManager gridManager;
    [Inject] private CommandManager commandManager;

    private Opponent currentPlayer;
    private GameContext gameContext;
    public int MinPlayers { get; private set; }


    public GameBoard(OpponentManager opponentManager, GridManager gridManager, CommandManager commandManager) {
        gameContext = new GameContext { gameBoard = this, _gridManager = gridManager };
        this.gridManager = gridManager;
        this.opponentManager = opponentManager;
    }

    public Opponent GetCurrentPlayer() {
        return currentPlayer;
    }

    // Used by other classes to allow start game
    public bool StartGame(int minPlayers = 2) {
        if (opponentManager.IsAllRegistered()) {
            currentPlayer = ChooseFirstPlayer();
            OnTurnBegan?.Invoke(currentPlayer);
            return true;
        } else {
            Debug.Log($"Can't start game because there are only {opponentManager.registeredOpponents.Count} registered players. Need: {MinPlayers}");
            return false;
        }
    }

    private Opponent ChooseFirstPlayer() {
        // Logic to choose the first player, e.g., randomly
        currentPlayer = opponentManager.GetRandomOpponent();
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
        await commandManager.ExecuteCommands();
    }

    private void GatherPlayerCreaturesActions(Opponent opponent) {
        foreach (var column in gridManager.MainGrid.Fields) {
            if (column == null) {
                Debug.LogWarning("Column with null");
                continue;
            }
            foreach (var field in column) {
                var creature = field.OccupiedCreature;
                if (creature != null) {
                    gameContext.initialField = field;
                    commandManager.RegisterCommand(creature.GetTurnActions(gameContext));
                }
            }
            gameContext.initialField = null;
        }

        ChangeTurn();
    }

    private void ChangeTurn() {
        currentPlayer = opponentManager.GetNextOpponent(currentPlayer);
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


    private bool ValidateFieldOccupy(Field field, Creature creature) {
        bool fieldExists = gridManager.MainGrid.FieldExists(field);
        if (!fieldExists) {
            Debug.LogWarning($"{field} doesnt exist in main grid : ");
            return fieldExists;
        }

        bool validOwner = field.Owner != null;
        if (!validOwner) {
            Debug.LogWarning($"{field} can`t be occupied by this owner! ");
            return validOwner;
        }

        return true;
    }
}
