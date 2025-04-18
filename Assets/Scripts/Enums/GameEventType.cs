public enum GameEventType {
    UNKNOWN_EVENT,
    // ��䳿, �� ���������� ���
    BATTLE_START,
    BATTLE_END,
    BATTLE_PAUSE,  // ������ ��� ������������ ���
    BATTLE_RESUME, // ³��������� ��� ���� �����
    ON_TURN_START,
    ON_TURN_END,

    // ��䳿 ����
    ON_CARD_BATTLE,  // ������ �������� � ��
    ON_CARD_DRAWN,   // ������ ���� ���������
    ON_CARD_DISCARDED, // ������ ���� ��������
    ON_CARD_PLAY,     // ������ ���� ������
    ON_CARD_DIE,      // ������ ���� �������
    ON_CARD_CLICKED,  // ������ ���� ������� (��� �����䳿 � ������������)

    ON_CREATURE_SUMMONED,
    ON_CREATURE_MOVED,
    ON_CREATURE_ATTACK,
    ON_CREATURE_HURT,
    ON_CREATURE_DEATH,

    // ��䳿 ��� �����䳿 � ������ �������� ��� ��� �������� ���
    ON_ENCOUNTER,       // ������� ������� � ����� ������������� ��� ����� ���������
    ON_PLAYER_WIN,      // ������� ������
    ON_PLAYER_LOSE,     // ������� �������
    ON_PLAYER_DRAW,     // ͳ��� (���� � ���� �����)

    // ��䳿, ���'����� � �������� ����
    ON_EFFECT_APPLIED,    // ����������� ����� ������
    ON_EFFECT_REMOVED,    // ����� ������ ��� ��������� (���������, ����� ��������� ��� ����)
    ON_CARD_EXHAUSTED,    // ������ ���� ����������� �� ����� �� ���� ���� ����������� ����� ����
    ON_CARD_SHUFFLED,     // ������ ���� ��������� � �����

    // ��䳿 ��� ��������� ��������� ���
    ON_RESOURCE_GAINED,   // ������� ������� ������� (����, ������ � �.�.)
    ON_RESOURCE_SPENT,    // ������� �������� ������� ��� ��������� 䳿 (���������, ������� ����)
    ON_DECK_SHUFFLED,     // ������ ���� ���������

    // ��䳿 ��� ������, ���'����� � ����� �����
    ON_CARD_FLIPPED,      // ������ ���� ����������� (������� � ��� ����� � ����� �������)
    ON_CARD_UPGRADED,     // ������ ���� ������������ ��� �������� � ��������� ������

    ON_GAME_START,        // ������� ���
    ON_GAME_OVER,         // ���������� ���
    ON_PLAYER_TURN_START, // ������� ���� ������
    ON_PLAYER_TURN_END,   // ʳ���� ���� ������
    ON_PLAYER_PASS,       // ������� ��������� ���
    ON_CARD_REVEALED,     // ������ ���� �������� (������� � ��������� ����� �������� �� ������)

    ON_TRIGGER_ABILITY,   // ��������� ���������� ��������� ������ �� ������
    ON_COMBO_PLAY,         // ����� � �������� ����� � ��, ���� ����� ���� �������� ����� ��� ����������� ������
    ON_DEBUFF_APPLIED,    // �� ������ ��� ������ ���� ����������� ������
    ON_BUFF_APPLIED,      // �� ������ ��� ������ ���� ����������� ����
    ON_GAME_STATE_SAVED,  // ���� ��� ��� ���������� (���������, ��� ����������� ��� ����������)
    ON_GAME_STATE_LOADED, // ���� ��� ��� ������������ � ������������ ����������
    ON_VICTORY_POINTS_GAINED, // ������� ������� ��������� ����
    ON_CARD_REMOVED,
    CREATURES_ACTIONED,
}
