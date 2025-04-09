using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
// This class will handle all actions: Creatures abilities, moves, spells, opponent end turn and other battle actions, Even Dialogues!
public class BatttleActionManager {

    [Inject] CommandManager _commandManager;
    // Zenject
    private GameEventBus _eventBus;

    private BoardAssigner _boardAssigner;
    public BatttleActionManager(GameEventBus eventBus, BoardAssigner boardAssigner, CommandManager commandManager) {
        _commandManager = commandManager;
        _boardAssigner = boardAssigner;
        _eventBus = eventBus;
        eventBus.SubscribeTo<TurnEndStartedEvent>(GenerateEndTurnActions);
    }

    public void GenerateEndTurnActions(ref TurnEndStartedEvent turnEndEvent) {
        List<Command> commands = new();

        List<Creature> creatures = _boardAssigner.GetOpponentCreatures(turnEndEvent.endTurnOpponent);

        foreach (var creature in creatures) {
            commands.Add(creature.GetEndTurnAction());
        }

        commands.Add(new CreaturesPerformedTurnsCommand(creatures, _eventBus));
        _commandManager.EnqueueCommands(commands);
    }
}

public class CreaturesPerformedTurnsCommand : Command {
    private List<Creature> creatures;
    private GameEventBus eventBus;

    public CreaturesPerformedTurnsCommand(List<Creature> creatures, GameEventBus eventBus) {
        this.creatures = creatures;
        this.eventBus = eventBus;
    }

    public async override UniTask Execute() {
        eventBus.Raise(new EndActionsExecutedEvent(creatures));
        await UniTask.CompletedTask;
    }

    public async override UniTask Undo() {
        await UniTask.CompletedTask;
    }
}

public struct EndActionsExecutedEvent : IEvent {
    private List<Creature> creatures;

    public EndActionsExecutedEvent(List<Creature> creatures) {
        this.creatures = creatures;
    }
}