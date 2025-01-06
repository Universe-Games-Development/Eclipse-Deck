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
