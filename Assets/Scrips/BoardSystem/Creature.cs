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
        // Реакція на видалення поля, наприклад, переміщення на інше поле або помилка.
        Console.WriteLine($"Creature on field ({field.row}, {field.column}) is notified about its removal.");
        // Логіка для обробки видалення, наприклад:
        // - Вибір нового місця
        // - Знищення істоти
    }

    internal async UniTask PerformTurn(object gameContext) {
        throw new NotImplementedException();
    }
}
