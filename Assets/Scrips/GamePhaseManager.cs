using UnityEngine;

public class GamePhaseManager : MonoBehaviour {
    private IEventManager eventManager;

    private void Start() {
        eventManager = GetComponent<IEventManager>();
    }

    public void TriggerTurnStart(Opponent currentPlayer) {
        eventManager.TriggerEvent(EventType.ON_TURN_START, new GameContext());
    }

    public void TriggerTurnEnd(Opponent currentPlayer) {
        eventManager.TriggerEvent(EventType.ON_TURN_END, new GameContext());
    }

    public void TriggerDrawCards(Opponent drawer, int amount) {
        eventManager.TriggerEvent(EventType.ON_CARD_DRAWN, new GameContext());
    }

    public void TriggerBattleBegins(Opponent opponent1, Opponent opponent2) {
        eventManager.TriggerEvent(EventType.BATTLE_START, new GameContext());
    }

    public void TriggerCardDie(Card deadCard) {
        eventManager.TriggerEvent(EventType.ON_CARD_DIE, new GameContext());
    }

    public void TriggerCardEnters(Card playedCard) {
        eventManager.TriggerEvent(EventType.ON_CARD_PLAY, new GameContext());
    }

    public void TriggerCardHit(Card attacker, Card target) {
        eventManager.TriggerEvent(EventType.ON_CARD_BATTLE, new GameContext());
    }
}