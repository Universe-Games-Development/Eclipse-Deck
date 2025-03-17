
public class Player : Opponent {
    public Player(GameEventBus eventBus, CardManager cardManager, IActionFiller abilityInputter) : base(eventBus, cardManager, abilityInputter) {
        Name = "Player";
    }
}
