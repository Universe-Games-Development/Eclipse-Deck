using UnityEngine;

public class Player : Opponent {
    public Player(IEventManager eventManager, ResourceManager resourceManager) : base(eventManager, resourceManager) {
        Name = "Player";
    }
}
