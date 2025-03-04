using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
// This class will handle all actions: Creatures abilities, moves, spells, opponent end turn and other battle actions, Even Dialogues!
public class BatttleActionManager {

    [Inject] CommandManager commandManager;
    // Zenject
    private GameEventBus eventBus;

    private BoardAssigner _boardAssigner;
    private TurnManager _turnManager;

    [Inject]
    public void Construct(GameEventBus eventBus, BoardAssigner boardAssigner, TurnManager turnManager) {
        _boardAssigner = boardAssigner;
        _turnManager = turnManager;

        this.eventBus = eventBus;
        eventBus.SubscribeTo<TurnEndStartedEvent>(GenerateEndTurnActions);
    }

    public void GenerateEndTurnActions(ref TurnEndStartedEvent turnEndEvent) {
        List<Command> commands = new();

        List<Creature> creatures = _boardAssigner.GetOpponentCreatures(turnEndEvent.endTurnOpponent);

        foreach (var creature in creatures) {
            commands.Add(creature.GetEndTurnAction());
        }

        commands.Add(new CreaturesPerformedTurnsCommand(creatures, eventBus));
        commandManager.EnqueueCommands(commands);
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