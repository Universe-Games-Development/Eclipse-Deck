public class Player : Opponent {
    public Player(IEventQueue eventQueue, ResourceManager resourceManager) : base(eventQueue, resourceManager) {
        Name = "Player";
    }
}
