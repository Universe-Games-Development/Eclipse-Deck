using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

public class GameBoard {

    // Opponents will use it to be notified which one can perform turn now
    public Action<Opponent> OnTurnBegan;

    public BoardOverseer boardOverseer { get; private set; }
    public OpponentManager opponentManager { get; private set; }

    private Opponent currentPlayer;
    private GameContext gameContext;
    public int MinPlayers { get; private set; }

    public GameBoard(BoardSettings boardConfig) {
        MinPlayers = boardConfig.minPlayers;
        boardOverseer = new BoardOverseer(boardConfig);
        opponentManager = new OpponentManager(boardOverseer.GridManager);
        gameContext = new GameContext { gameBoard = this, overseer = boardOverseer };
    }

    public Opponent GetCurrentPlayer() {
        return currentPlayer;
    }

    // Used by other classes to allow start game
    public bool StartGame(int minPlayers = 2) {
        if (opponentManager.registeredOpponents.Count >= minPlayers) {
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
        currentPlayer = opponentManager.registeredOpponents[UnityEngine.Random.Range(0, opponentManager.registeredOpponents.Count)];
        Debug.Log($"{currentPlayer.Name} is chosen to start first.");
        return currentPlayer;
    }

    // Used by any opponent
    public async UniTask PerformTurn(Opponent opponent) {
        if (opponent != currentPlayer) {
            Debug.Log("Not your turn buddy");
            return;
        }
        await RunTurnAsync(currentPlayer);
    }

    private async UniTask RunTurnAsync(Opponent opponent) {
        if (!boardOverseer.IsInitialized()) {
            Debug.LogError("Overseer is not initialized.");
            return;
        }

        foreach (var column in boardOverseer.GridManager.MainGrid.Fields) {
            if (column == null) {
                Debug.LogWarning("Column with null");
                continue;
            }
            foreach (var field in column) {
                var creature = field.OccupiedCreature;
                if (creature != null) {
                    gameContext.initialField = field;
                    await creature.PerformTurn(gameContext);
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

    public void UpdateBoard(BoardSettings newBoardConfig) {
        boardOverseer.UpdateBoard(newBoardConfig);
    }

    // used by Opponents to summon
    public async UniTask<bool> SummonCreature(Opponent summoner, Field field, Creature creature) {
        return await PlaceCreatureInternal(field, creature, summoner);
    }

    // used by creatures to move
    public async UniTask<bool> PlaceCreature(Field field, Creature creature) {
        return await PlaceCreatureInternal(field, creature, null);
    }

    private async UniTask<bool> PlaceCreatureInternal(Field field, Creature creature, Opponent owner) {
        // Перевірка, чи поле є в mainGrid
        bool isValidOccupy = ValidateFieldOccupy(field, creature);
        if (!isValidOccupy) {
            Debug.LogWarning($"Failed to place creature: Field does not exist or is not owned by {owner?.Name}");
            return false;
        }

        bool result = owner != null
            ? await field.SummonCreatureAsync(creature, owner)
            : await field.PlaceCreatureAsync(creature);

        if (result) {
            Debug.Log($"Creature placed successfully in the field {field.row} / {field.column}");
            return true;
        } else {
            Debug.Log("Field cannot spawn creature");
            return false;
        }
    }

    private bool ValidateFieldOccupy(Field field, Creature creature) {
        bool fieldExists = boardOverseer.mainNavigator.FieldExists(field);
        bool validOwner = field.Owner != null;
        return fieldExists && validOwner;
    }
}
