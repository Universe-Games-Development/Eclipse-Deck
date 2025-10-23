using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Damage", menuName = "Operations/Damage")]
public class DamageOperationData : OperationData {
    public int damage = 6;
    [SerializeField] private Fireball fireballPrefab;

    public override GameOperation CreateOperation(IOperationFactory factory, TargetRegistry targetRegistry) {
        IHealthable healthable = targetRegistry.Get<IHealthable>(TargetKeys.MainTarget);

        return factory.Create<DamageOperation>(this, healthable);
    }

    protected override void BuildDefaultRequirements() {
        AddRequirement(RequirementPresets.Damageble(TargetKeys.MainTarget));
    }
}

public class DamageOperation : GameOperation {
    private readonly DamageOperationData data;
    private IHealthable target;

    public DamageOperation(DamageOperationData data, IHealthable target) {
        this.data = data;
        this.target = target;
    }

    public override bool Execute() {
        // Тепер target вже має тип IHealthable!
        target.TakeDamage(data.damage);
        return true;
    }
}
