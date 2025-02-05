public class Player : Opponent {
    public Player(IEventQueue eventQueue, AssetLoader assetLoader) : base(eventQueue, assetLoader) {
        Name = "Player";
    }
}
