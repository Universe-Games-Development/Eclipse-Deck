using Cysharp.Threading.Tasks;
using System;

public class Creature {
    public Health Health;
    public Attack attack;

    private Card card;
    private CreatureMovementHandler movementHandler;

    public Creature(Card myCard, CreatureSO creatuseSO) {
        card = myCard;
        var movementData = creatuseSO.movementData;
        // TO DO : abilities initialization

        movementHandler = new CreatureMovementHandler(movementData, this);
    }

    // TODO: Ensure that same creature don`t make a move after moveing (GameBoard calls in columns so it may call again moved creture)
    public async UniTask PerformTurn(GameContext gameContext) {
        gameContext.currentCreature = this;
        await movementHandler.ExecuteMovement(gameContext);
        gameContext.currentCreature = null;
    }

    public void OnFieldRemoved(Field field) {
        // Реакція на видалення поля, наприклад, переміщення на інше поле або помилка.
        Console.WriteLine($"Creature on field ({field.row}, {field.column}) is notified about its removal.");
        // - Вибір нового місця
        // - Знищення істоти
    }
}