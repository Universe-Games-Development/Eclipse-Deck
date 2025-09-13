using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;

public abstract class OperationData : ScriptableObject {
}

public class DamageOperationData : OperationData {
    public int damage = 6;
    [SerializeField] private Fireball fireballPrefab;
}

[OperationFor(typeof(DamageOperationData))]
public class DamageOperation : GameOperation {
    private const string TargetCreatureKey = "targetCreature";
    private readonly DamageOperationData _data;

    public DamageOperation(DamageOperationData data) {
        _data = data;

        var anyDamagableEnemyTarget = TargetRequirements.EnemyDamageable;

        RequestTargets.Add(new Target(TargetCreatureKey, anyDamagableEnemyTarget)
        );
    }

    public override bool Execute() {
        if (!TryGetTarget(TargetCreatureKey, out UnitPresenter damagablePresenter)) {
            Debug.LogError($"Valid {TargetCreatureKey} not found for damage operation");
            return false;
        }

        IHealthable target = damagablePresenter as IHealthable;

        target.Health.TakeDamage(_data.damage);
        return true;
    }
}

public interface IGameAnimation {
    UniTask<bool> LoadResources();
    UniTask PlayAnimation();
    void SkipAnimation();
}

public class FireballAnimation : IGameAnimation {
    private string prefabAddress;
    private UnitPresenter target;
    private Fireball fireballPrefab;

    public FireballAnimation(string prefabAddress, UnitPresenter target) {
        this.prefabAddress = prefabAddress;
        this.target = target;
    }

    public async UniTask<bool> LoadResources() {
        try {
            var handle = Addressables.LoadAssetAsync<Fireball>(prefabAddress);
            fireballPrefab = await handle.Task;
            return fireballPrefab != null;
        } catch {
            return false;
        }
    }

    public async UniTask PlayAnimation() {
        if (fireballPrefab == null) {
            SkipAnimation();
            return;
        }

        // Создаем и запускаем файрбол
        var fireball = Object.Instantiate(fireballPrefab);
        var fireballComponent = fireball.GetComponent<Fireball>();

        // Просто ждем завершения полета файрбола
        await fireballComponent.LaunchToTarget(target.transform);
    }

    public void SkipAnimation() {
        // При пропуске анимации ничего особенного не делаем
        // Логика уже выполнена, следующая анимация в очереди покажет результат
    }
}
