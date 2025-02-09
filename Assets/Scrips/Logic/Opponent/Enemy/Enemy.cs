public class Enemy : Opponent {
    public Enemy(IEventQueue eventQueue, AssetLoader assetLoader, ICommandFiller commandFiller, CommandManager commandManager) : base(eventQueue, assetLoader, commandFiller, commandManager) {
        Name = "Enemy";
    }
}
