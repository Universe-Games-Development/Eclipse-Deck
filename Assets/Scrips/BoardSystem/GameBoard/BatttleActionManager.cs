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
        if (data is not TurnEndEvent turnEndEventData) {
            Debug.LogWarning("Received invalid event data in BattleActionManager.");
            return null;
        }
        return GetCreaturesActions(turnEndEventData);
    }

    public List<ICommand> GetCreaturesActions(TurnEndEvent turnEndEventData) {
        
        List<ICommand> commands = new();

        foreach (var creature in _boardAssigner.GetOpponentCreatures(_turnManager.ActiveOpponent)) {
            commands.Add(creature.GetEndTurnMove(turnEndEventData));
        }

        return commands;
    }
}
