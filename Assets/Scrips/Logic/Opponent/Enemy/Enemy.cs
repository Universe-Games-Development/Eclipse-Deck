using Unity.VisualScripting;

public class Enemy : Opponent {
    public Enemy(GameEventBus eventBus, AssetLoader assetLoader, IAbilityInputter abilityInputter, CommandManager commandManager) : base(eventBus, assetLoader, abilityInputter, commandManager) {
        Name = "Enemy";
    }
}
