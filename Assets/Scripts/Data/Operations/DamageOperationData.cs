using Cysharp.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "Damage", menuName = "Operations/Damage")]
public class DamageOperationData : OperationData<DamageOperation> {
    public int damage = 6;
    [SerializeField] private Fireball fireballPrefab;
}

public class DamageOperation : GameOperation {
    private readonly DamageOperationData data;

    public DamageOperation(UnitModel source, DamageOperationData data) : base(source) {
        this.data = data;

        AddTarget(new TargetInfo(TargetKeys.Target, TargetRequirements.EnemyCreature));
    }

    public override async UniTask<bool> Execute() {
        // Type-safe отримання target без кастів!
        if (!TryGetTypedTarget<IHealthable>(TargetKeys.Target, out var target)) {
            Debug.LogError($"Valid {TargetKeys.Target} not found for damage operation");
            return false;
        }

        // Тепер target вже має тип IHealthable!
        target.TakeDamage(data.damage);
        await UniTask.DelayFrame(1);
        return true;
    }
}
