public class GameContext {
    public Opponent activePlayer;
    public Opponent opponentPlayer;
    public Card sourceCard;  // Карта, яка активує здібність (нападник)
    public Card targetCard;  // Карта, на яку застосовується здібність (ціль)
    public BattleCreature sourceCreature;  // Карта, яка активує здібність (нападник)
    public BattleCreature targetCreature;  // Карта, на яку застосовується здібність (ціль)
    public int damage;       // Шкода, яка завдається
    public int healAmount;   // Кількість лікування
    public int buffAmount;   // Кількість підсилення (для buff- здібностей)
}

public class GameContextManager {
    private GameContext currentContext;

    public GameContextManager() {
        currentContext = new GameContext();  // Ініціалізація нового контексту
    }

    // Оновлення контексту після події
    public void UpdateContext(Card sourceCard, Card targetCard, int damage, int healAmount, int buffAmount) {
        currentContext.sourceCard = sourceCard;
        currentContext.targetCard = targetCard;
        currentContext.damage = damage;
        currentContext.healAmount = healAmount;
        currentContext.buffAmount = buffAmount;
    }

    // Отримання поточного контексту
    public GameContext GetCurrentContext() {
        return currentContext;
    }

    // Оновлення контексту після події
    public void OnEventOccurred(EventType eventType) {
        // Тут можна додати додаткові дії для обробки події
        // Наприклад, якщо це подія атак, оновлюємо шкоду або інші параметри
        switch (eventType) {
            case EventType.ON_CARD_PLAY:
                // Приклад оновлення контексту після події активації карти
                if (currentContext.sourceCard != null && currentContext.targetCard != null) {
                    UpdateContext(currentContext.sourceCard, currentContext.targetCard,
                        currentContext.sourceCard.Attack, 0, 0);
                }
                break;

                // Додаткові випадки для інших подій
                // case EventType.ON_CARD_ATTACK:
                // case EventType.ON_CARD_HEAL:
                // тощо
        }
    }
}
