using Zenject;

public class Player : Opponent {
    public Player(GameEventBus eventBus, AssetLoader assetLoader, IActionFiller abilityInputter, CommandManager commandManager) : base(eventBus, assetLoader, abilityInputter, commandManager) {
        Name = "Player";
    }
}
