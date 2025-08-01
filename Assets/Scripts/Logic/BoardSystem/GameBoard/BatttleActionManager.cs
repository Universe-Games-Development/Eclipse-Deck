using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
// This class will handle all actions: Creatures abilities, moves, spells, opponent end turn and other battle actions, Even Dialogues!
public class BatttleActionManager {

    [Inject] CommandManager _commandManager;
    // Zenject
    private GameEventBus _eventBus;
    public BatttleActionManager(GameEventBus eventBus, CommandManager commandManager) {
        _commandManager = commandManager;
        _eventBus = eventBus;
    }
}
