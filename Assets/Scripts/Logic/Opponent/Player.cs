using Zenject;

public class Player : Opponent {
    public Player(GameEventBus eventBus, AssetLoader assetLoader, IActionFiller abilityInputter) : base(eventBus, assetLoader, abilityInputter) {
        Name = "Player";
    }
}
