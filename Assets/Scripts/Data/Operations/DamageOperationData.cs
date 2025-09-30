using Cysharp.Threading.Tasks;
using UnityEngine;

public class DamageOperationData : OperationData {
    public int damage = 6;
    [SerializeField] private Fireball fireballPrefab;
}

[OperationFor(typeof(DamageOperationData))]
public class DamageOperation : GameOperation {
    private const string TargetKey = "target";
    private readonly DamageOperationData data;

    public DamageOperation(DamageOperationData data) {
        this.data = data;

        AddTarget(new TargetInfo(TargetKey, TargetRequirements.EnemyHealthable));
    }

    public override async UniTask<bool> Execute() {
        // Type-safe отримання target без кастів!
        if (!TryGetTypedTarget<IHealthable>(TargetKey, out var target)) {
            Debug.LogError($"Valid {TargetKey} not found for damage operation");
            return false;
        }

        // Тепер target вже має тип IHealthable!
        target.Health.TakeDamage(data.damage);
        await UniTask.DelayFrame(1);
        return true;
    }
}