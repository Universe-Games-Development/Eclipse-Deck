using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
// This class will handle all actions: Creatures abilities, moves, spells, opponent end turn and other battle actions, Even Dialogues!
public class BatttleActionManager {

    [Inject] CommandManager _commandManager;
    // Zenject
    private IEventBus<IEvent> _eventBus;
    public BatttleActionManager(IEventBus<IEvent> eventBus, CommandManager commandManager) {
        _commandManager = commandManager;
        _eventBus = eventBus;
    }
}
