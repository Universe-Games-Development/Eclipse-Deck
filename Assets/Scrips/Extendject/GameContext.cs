public class GameContext {
    public Opponent activePlayer;
    public Opponent opponentPlayer;

    public Card sourceCard;
    public Card targetCard;

    public Field sourceField;
    public Field initialField;
    public Field targetField;

    public int damage;
    public int healAmount;
    public int buffAmount;

    public Creature currentCreature;
    

    // By zenject
    public GameBoard gameBoard;
    public BoardOverseer overseer;
}
