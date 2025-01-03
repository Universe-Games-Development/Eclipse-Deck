public class Ability : IEventListener {
    private AbilitySO abilitySO;
    private Card card;
    private IEventManager eventManager;
    public CardState activationState;
    public CardState deactivationState;

    public Ability(AbilitySO abilitySO, Card card, IEventManager eventManager) {
        this.abilitySO = abilitySO;
        activationState = abilitySO.activationState;
        deactivationState = abilitySO.deactivationState;

        this.card = card;
        this.eventManager = eventManager;
    }

    public void RegisterAbility() {
        if (eventManager == null) return;
        eventManager.RegisterListener(this, EventType.ON_CARD_PLAY);
    }

    public void UnregisterAbility() {
        if (eventManager == null) return;
        eventManager.UnregisterListener(this, EventType.ON_CARD_PLAY);
    }

    public void OnEvent(EventType eventType, GameContext gameContext) {
        abilitySO.ActivateAbility(gameContext);
    }
}
