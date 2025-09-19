using UnityEngine;
using Zenject;

public class PoolInstaller : MonoInstaller {
    [Header("Pool Prefabs")]
    public PoolManager poolManagerPrefab;
    public ComponentPool<Card3DView> card3DPoolPrefab;
    public ComponentPool<Card3DView> creaturePoolPrefab;

    public override void InstallBindings() {
        // Реєструємо PoolManager як синглтон
        Container.Bind<PoolManager>()
            .FromComponentInNewPrefab(poolManagerPrefab)
            .AsSingle()
            .NonLazy();

        // Реєструємо пули
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