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

public class GameContextManager {
    private GameContext currentContext;

    public GameContextManager() {
        currentContext = new GameContext();  // ����������� ������ ���������
    }

    // ��������� ��������� ���� ��䳿
    public void UpdateContext(Card sourceCard, Card targetCard, int damage, int healAmount, int buffAmount) {
        currentContext.sourceCard = sourceCard;
        currentContext.targetCard = targetCard;
        currentContext.damage = damage;
        currentContext.healAmount = healAmount;
        currentContext.buffAmount = buffAmount;
    }

    // ��������� ��������� ���������
    public GameContext GetCurrentContext() {
        return currentContext;
    }

    // ��������� ��������� ���� ��䳿
    public void OnEventOccurred(EventType eventType) {
        // ��� ����� ������ �������� 䳿 ��� ������� ��䳿
        // ���������, ���� �� ���� ����, ��������� ����� ��� ���� ���������
        switch (eventType) {
            case EventType.ON_CARD_PLAY:
                // ������� ��������� ��������� ���� ��䳿 ��������� �����
                if (currentContext.sourceCard != null && currentContext.targetCard != null) {
                    UpdateContext(currentContext.sourceCard, currentContext.targetCard,
                        currentContext.sourceCard.Attack, 0, 0);
                }
                break;

                // �������� ������� ��� ����� ����
                // case EventType.ON_CARD_ATTACK:
                // case EventType.ON_CARD_HEAL:
                // ����
        }
    }
}
