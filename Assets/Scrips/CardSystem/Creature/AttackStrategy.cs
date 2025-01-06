public abstract class AttackStrategy {
    public abstract void Attack(Field currentField, Field[] enemyFields, int attackPower);
}

public class SingleAttack : AttackStrategy {
    public override void Attack(Field currentField, Field[] enemyFields, int attackPower) {
        int targetIndex = currentField.Index; // Поле навпроти
        if (targetIndex >= 0 && targetIndex < enemyFields.Length) {
            enemyFields[targetIndex].ReceiveAttack(attackPower);
        }
    }
}

public class WideAttack : AttackStrategy {
    public override void Attack(Field currentField, Field[] enemyFields, int attackPower) {
        for (int i = -1; i <= 1; i++) {
            int targetIndex = currentField.Index + i;
            if (targetIndex >= 0 && targetIndex < enemyFields.Length) {
                enemyFields[targetIndex].ReceiveAttack(attackPower);
            }
        }
    }
}

public class DiagonalAttack : AttackStrategy {
    public override void Attack(Field currentField, Field[] enemyFields, int attackPower) {
        int[] diagonals = { currentField.Index - 1, currentField.Index + 1 };
        foreach (var targetIndex in diagonals) {
            if (targetIndex >= 0 && targetIndex < enemyFields.Length) {
                enemyFields[targetIndex].ReceiveAttack(attackPower);
            }
        }
    }
}