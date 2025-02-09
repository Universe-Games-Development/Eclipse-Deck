using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
// This class will handle all actions: Creatures abilities, moves, spells, opponent end turn and other battle actions, Even Dialogues!
public class BatttleActionManager : IEventListener {

    // Zenject
    private EventQueue _eventQueue;

    private GameBoard _gameBoard;
    private BoardAssigner _boardAssigner;
    private TurnManager _turnManager;

    [Inject]
    public void Construct(EventQueue eventQueue, GameBoard gameBoard, BoardAssigner boardAssigner, TurnManager turnManager) {
        _gameBoard = gameBoard;
        _boardAssigner = boardAssigner;
        _turnManager = turnManager;

        _eventQueue = eventQueue;
        eventQueue.RegisterListener(this, EventType.ON_TURN_END);
    }


    //IEventListener
    public object OnEventReceived(object data) {
        if (data is not TurnChangeEventData turnChangeEventData) {
            Debug.LogWarning("Received invalid event data in BattleActionManager.");
            return null;
        }
        
        return GetEndTurnActions(turnChangeEventData);
    }

    public List<ICommand> GetEndTurnActions(TurnChangeEventData turnChangeEventData) {
        
        List<ICommand> commands = new();

        List<Creature> creatures = _boardAssigner.GetOpponentCreatures(turnChangeEventData.activeOpponent);

        foreach (var creature in creatures) {
            commands.Add(creature.GetEndTurnMove(turnChangeEventData));
        }

        commands.Add(new CreaturesPerformedTurnsCommand(creatures, _eventQueue));

        return commands;
    }
}

public class CreaturesPerformedTurnsCommand : ICommand {
    private List<Creature> creatures;
    private EventQueue eventQueue;

    public CreaturesPerformedTurnsCommand(List<Creature> creatures, EventQueue eventQueue) {
        this.creatures = creatures;
        this.eventQueue = eventQueue;
    }

    public async UniTask Execute() {
        eventQueue.TriggerEvent(EventType.CREATURES_ACTIONED, new CreaturesPerformedTurnsData(creatures));
    }

    public async UniTask Undo() {
        throw new System.NotImplementedException();
    }
}
