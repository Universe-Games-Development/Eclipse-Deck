public class Enemy : Opponent {
    public Enemy(IEventQueue eventQueue, ResourceManager resourceManager) : base(eventQueue, resourceManager) {
        Name = "Enemy";
    }
}
