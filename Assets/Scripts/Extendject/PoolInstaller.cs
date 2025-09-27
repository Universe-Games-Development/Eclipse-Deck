using UnityEngine;
using Zenject;

public class PoolInstaller : MonoInstaller {
    [Header("Pool Prefabs")]
    public PoolManager poolManagerPrefab;
    public ComponentPool<CardView> cardPoolPrefab;
    public ComponentPool<CreatureView> creaturePoolPrefab;

    public override void InstallBindings() {
        // Реєструємо PoolManager як синглтон
        Container.Bind<PoolManager>()
            .FromComponentInNewPrefab(poolManagerPrefab)
            .AsSingle()
            .NonLazy();

        // Реєструємо пули
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