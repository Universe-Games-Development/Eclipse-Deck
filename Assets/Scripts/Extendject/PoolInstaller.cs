using UnityEngine;
using Zenject;

public class PoolInstaller : MonoInstaller {
    [Header("Pool Prefabs")]
    public GameObject poolManagerPrefab;
    public GameObject card3DPoolPrefab;
    public GameObject creaturePoolPrefab;

    public override void InstallBindings() {
        // �������� PoolManager �� ��������
        Container.Bind<PoolManager>()
            .FromComponentInNewPrefab(poolManagerPrefab)
            .AsSingle()
            .NonLazy();

        // �������� ����
        Container.Bind<IComponentPool<Card3DView>>()
            .To<Card3DPool>()
            .FromComponentInNewPrefab(card3DPoolPrefab)
            .AsSingle();

        Container.Bind<IComponentPool<CreatureView>>()
            .To<CreaturePool>()
            .FromComponentInNewPrefab(creaturePoolPrefab)
            .AsSingle();
    }
}