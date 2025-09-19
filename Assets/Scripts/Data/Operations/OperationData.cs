using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class OperationData : ScriptableObject {
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

        // ������� � ��������� �������
        var fireball = Object.Instantiate(fireballPrefab);
        var fireballComponent = fireball.GetComponent<Fireball>();

        // ������ ���� ���������� ������ ��������
        await fireballComponent.LaunchToTarget(target.transform);
    }

    public void SkipAnimation() {
        // ��� �������� �������� ������ ���������� �� ������
        // ������ ��� ���������, ��������� �������� � ������� ������� ���������
    }
}
