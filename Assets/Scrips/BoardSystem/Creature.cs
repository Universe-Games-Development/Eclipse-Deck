using Cysharp.Threading.Tasks;
using System;
using System.Threading.Tasks;

public class Creature {
    public Health Health;
    public Attack attack;
    private Card card;

    public Creature(Card myCard) {
        card = myCard;
    }

    public void OnFieldRemoved(Field field) {
        // ������� �� ��������� ����, ���������, ���������� �� ���� ���� ��� �������.
        Console.WriteLine($"Creature on field ({field.row}, {field.column}) is notified about its removal.");
        // ����� ��� ������� ���������, ���������:
        // - ���� ������ ����
        // - �������� ������
    }

    internal async UniTask PerformTurn(object gameContext) {
        throw new NotImplementedException();
    }
}
