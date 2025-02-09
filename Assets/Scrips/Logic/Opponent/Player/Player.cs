using System;

public class Player : Opponent {
    public Player(IEventQueue eventQueue, AssetLoader assetLoader, ICommandFiller commandFiller, CommandManager commandManager) : base(eventQueue, assetLoader, commandFiller, commandManager) {
        Name = "Player";
    }
}
