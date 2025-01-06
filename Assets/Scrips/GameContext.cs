public class GameContext {
    public Opponent activePlayer;
    public Opponent opponentPlayer;
    public Card sourceCard;  // �����, ��� ������ ������� (��������)
    public Card targetCard;  // �����, �� ��� ������������� ������� (����)
    public BattleCreature sourceCreature;  // �����, ��� ������ ������� (��������)
    public BattleCreature targetCreature;  // �����, �� ��� ������������� ������� (����)
    public int damage;       // �����, ��� ���������
    public int healAmount;   // ʳ������ ��������
    public int buffAmount;   // ʳ������ ��������� (��� buff- ���������)
}
