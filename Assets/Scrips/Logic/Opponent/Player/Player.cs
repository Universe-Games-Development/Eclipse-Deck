using System;

public class Player : Opponent {
    public Player(GameEventBus eventBus, AssetLoader assetLoader, IAbilityInputter abilityInputter, CommandManager commandManager) : base(eventBus, assetLoader, abilityInputter, commandManager) {
        Name = "Player";
    }
}
