using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Zenject;
// This class will handle all actions: Creatures abilities, moves, spells, opponent end turn and other battle actions, Even Dialogues!
public class BatttleActionManager {

    [Inject] CommandManager commandManager;
    // Zenject
    private GameEventBus eventBus;

    private GameBoard _gameBoard;
    private BoardAssigner _boardAssigner;
    private TurnManager _turnManager;

    [Inject]
    public void Construct(GameEventBus eventBus, GameBoard gameBoard, BoardAssigner boardAssigner, TurnManager turnManager) {
        _gameBoard = gameBoard;
        _boardAssigner = boardAssigner;
        _turnManager = turnManager;

        this.eventBus = eventBus;
        eventBus.SubscribeTo<TurnEndEvent>(GenerateEndTurnActions);
    }

    public void GenerateEndTurnActions(ref TurnEndEvent turnEndEvent) {
        
        List<ICommand> commands = new();

        List<Creature> creatures = _boardAssigner.GetOpponentCreatures(turnEndEvent.endTurnOpponent);

        foreach (var creature in creatures) {
            commands.Add(creature.GetEndTurnMove());
        }

        commands.Add(new CreaturesPerformedTurnsCommand(creatures, eventBus));
        commandManager.EnqueueCommands(commands);
    }
}

public class CreaturesPerformedTurnsCommand : ICommand {
    private List<Creature> creatures;
    private GameEventBus eventBus;

    public CreaturesPerformedTurnsCommand(List<Creature> creatures, GameEventBus eventBus) {
        this.creatures = creatures;
        this.eventBus = eventBus;
    }

    public async UniTask Execute() {
        eventBus.Raise(new EndActionsExecutedEvent(creatures));
        await UniTask.CompletedTask;
    }

    public async UniTask Undo() {
        throw new System.NotImplementedException();
    }
}

public struct EndActionsExecutedEvent : IEvent {
    private List<Creature> creatures;

    public EndActionsExecutedEvent(List<Creature> creatures) {
        this.creatures = creatures;
    }
}