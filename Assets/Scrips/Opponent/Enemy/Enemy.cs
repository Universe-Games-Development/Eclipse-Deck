public class Enemy : Opponent {
    public Enemy(IEventQueue eventQueue, AssetLoader assetLoader) : base(eventQueue, assetLoader) {
        Name = "Enemy";
    }
}
