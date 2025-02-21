using Zenject;

public class Enemy : Opponent {
    public Enemy(GameEventBus eventBus, AssetLoader assetLoader, IActionFiller abilityInputter, CommandManager commandManager) : base(eventBus, assetLoader, abilityInputter, commandManager) {
        Name = "Enemy";
    }
}
