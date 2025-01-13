public class GameContext {
    public Opponent activePlayer;
    public Opponent opponentPlayer;
    public Card sourceCard;
    public Card targetCard;
    public Field sourceField;
    public Field targetField;
    public int damage;
    public int healAmount;
    public int buffAmount;
    internal BoardOverseer overseer;
    internal Creature currentCreature;
    internal Field currentField;

    public GameBoard gameBoard { get; internal set; }
}
