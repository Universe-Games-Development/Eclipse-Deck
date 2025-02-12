using System;

public class Player : Opponent {
    public Player(GameEventBus eventBus, AssetLoader assetLoader, ICommandFiller commandFiller, CommandManager commandManager) : base(eventBus, assetLoader, commandFiller, commandManager) {
        Name = "Player";
    }
}
