using UnityEngine;

public class GamePhaseManager : MonoBehaviour {
    private IEventManager eventManager;

    private void Start() {
        eventManager = GetComponent<IEventManager>();
    }

    public void TriggerTurnStart(Opponent currentPlayer) {
        eventManager.TriggerEventAsync(EventType.ON_TURN_START, new GameContext());
    }

    public void TriggerTurnEnd(Opponent currentPlayer) {
        eventManager.TriggerEventAsync(EventType.ON_TURN_END, new GameContext());
    }

    public void TriggerDrawCards(Opponent drawer, int amount) {
        eventManager.TriggerEventAsync(EventType.ON_CARD_DRAWN, new GameContext());
    }

    public void TriggerBattleBegins(Opponent opponent1, Opponent opponent2) {
        eventManager.TriggerEventAsync(EventType.BATTLE_START, new GameContext());
    }

    public void TriggerCardDie(Card deadCard) {
        eventManager.TriggerEventAsync(EventType.ON_CARD_DIE, new GameContext());
    }

    public void TriggerCardEnters(Card playedCard) {
        eventManager.TriggerEventAsync(EventType.ON_CARD_PLAY, new GameContext());
    }

    public void TriggerCardHit(Card attacker, Card target) {
        eventManager.TriggerEventAsync(EventType.ON_CARD_BATTLE, new GameContext());
    }
}