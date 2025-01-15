using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

public class GameBoard {

    // Opponents will use it to be notified which one can perform turn now
    public Action<Opponent> OnTurnBegan;

    public BoardOverseer boardOverseer { get; private set; }
    private List<Opponent> registeredOpponents;
    private Opponent currentPlayer;
    private GameContext gameContext;
    public int MinPlayers { get; private set; }


    public GameBoard(BoardSettings boardConfig) {
        MinPlayers = boardConfig.minPlayers;
        boardOverseer = new BoardOverseer(boardConfig);
        registeredOpponents = new List<Opponent>();
        gameContext = new GameContext { gameBoard = this, overseer = boardOverseer };
    }

    public void RegisterOpponent(Opponent opponent) {
        if (!registeredOpponents.Contains(opponent)) {
            registeredOpponents.Add(opponent);
            boardOverseer.OccupyGrid(opponent);
            opponent.OnDefeat += UnRegisterOpponent;
            Debug.Log($"Opponent {opponent.Name} registered.");
        }
    }

    public void UnRegisterOpponent(Opponent opponent) {
        if (registeredOpponents.Contains(opponent)) {
            registeredOpponents.Remove(opponent);
            opponent.OnDefeat -= UnRegisterOpponent;
            Debug.Log($"Opponent {opponent.Name} unregistered.");
        }
    }

    public Opponent GetCurrentPlayer() {
        return currentPlayer;
    }

    // Used by other classes to allow start game
    public bool StartGame(int minPlayers = 2) {
        if (registeredOpponents.Count >= minPlayers) {
            currentPlayer = ChooseFirstPlayer();
            OnTurnBegan?.Invoke(currentPlayer);
            return true;
        } else {
            Debug.Log($"Can`t start game because there are only {registeredOpponents.Count} registered players. " + "Need :" + MinPlayers);
            return false;
        }
    }

    private Opponent ChooseFirstPlayer() {
        // Logic to choose the first player, e.g., randomly
        currentPlayer = registeredOpponents[UnityEngine.Random.Range(0, registeredOpponents.Count)];
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
        if (boardOverseer.MainGrid.Fields == null || boardOverseer.MainGrid.Fields.Count == 0) {
            Debug.LogError("MainGrid fields are not initialized.");
            return;
        }

        foreach (var column in boardOverseer.MainGrid.Fields) {
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
        if (registeredOpponents.Count == 0) {
            Debug.LogError("No players left to take a turn.");
            return;
        }

        int currentIndex = registeredOpponents.IndexOf(currentPlayer);
        currentPlayer = registeredOpponents[(currentIndex + 1) % registeredOpponents.Count];

        Debug.Log($"It is now {currentPlayer.Name}'s turn.");
        if (currentPlayer == null) {
            Debug.Log("ChangeTurn: Cant find player");
            return;
        }
        OnTurnBegan?.Invoke(currentPlayer);
    }

    public void UpdateBoard(BoardSettings newBoardConfig) {
        boardOverseer.UpdateBoard(newBoardConfig);
    }

    // used by Opponents to summon
    public async UniTask<bool> SummonCreature(Opponent summoner, Field field, Creature creature) {
        // Перевірка, чи поле є в mainGrid
        bool isValidOccupy = ValidateFieldOccupy(field, creature);
        if (!isValidOccupy) {
            Debug.Log($"Failed to place creature: Field does not exist or is not owned by {summoner.Name}");
            return false;
        }

        bool result = await field.SummonCreatureAsync(creature, summoner); // Передбачаючи асинхронний метод SummonCreatureAsync
        if (result) {
            Debug.Log($"Creature placed successfully in the field owned by {summoner.Name}");
            return true;
        } else {

            Debug.Log("Field cannot spawn creature");
            return false;
        }
    }

    // used by creatures to move
    public async UniTask<bool> PlaceCreature(Field field, Creature creature) {
        // Перевірка, чи поле є в mainGrid
        bool isValidOccupy = ValidateFieldOccupy(field, creature);
        if (!isValidOccupy) {
            Debug.LogWarning($"Failed to place creature: Field does not exist");
            return false;
        }

        bool result = await field.PlaceCreatureAsync(creature); // Передбачаючи асинхронний метод SummonCreatureAsync
        if (result) {
            Debug.Log($"Creature placed successfully in the field {field.row} " + " / " + $"{field.column}");
            return true;
        } else {
            Debug.Log("Field cannot spawn creature");
            return false;
        }
    }

    private bool ValidateFieldOccupy(Field field, Creature creature) {
        bool fieldExists = boardOverseer.FieldExists(field);
        bool validOwner = field.Owner != null;
        return fieldExists && validOwner;
    }
}
