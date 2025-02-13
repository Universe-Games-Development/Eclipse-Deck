using Unity.VisualScripting;

public class Enemy : Opponent {
    public Enemy(GameEventBus eventBus, AssetLoader assetLoader, ICardsInputFiller commandFiller, CommandManager commandManager) : base(eventBus, assetLoader, commandFiller, commandManager) {
        Name = "Enemy";
    }
}
