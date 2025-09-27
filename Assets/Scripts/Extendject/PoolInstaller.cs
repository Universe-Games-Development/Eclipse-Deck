using UnityEngine;
using Zenject;

public class PoolInstaller : MonoInstaller {
    [Header("Pool Prefabs")]
    public PoolManager poolManagerPrefab;
    public ComponentPool<CardView> cardPoolPrefab;
    public ComponentPool<CreatureView> creaturePoolPrefab;

    public override void InstallBindings() {
        // �������� PoolManager �� ��������
        Container.Bind<PoolManager>()
            .FromComponentInNewPrefab(poolManagerPrefab)
            .AsSingle()
            .NonLazy();

        // �������� ����
        Container.Bind<IComponentPool<CardView>>()
            .To<CardPool>()
            .FromComponentInNewPrefab(cardPoolPrefab)
            .AsSingle();

        Container.Bind<IComponentPool<CreatureView>>()
            .To<CreaturePool>()
            .FromComponentInNewPrefab(creaturePoolPrefab)
            .AsSingle();
    }
}