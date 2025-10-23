using UnityEngine;
using Zenject;

public class PoolInstaller : MonoInstaller {
    [Header("Pool Prefabs")]
    public PoolManager poolManagerPrefab;
    public ComponentPool<CardView> cardPoolPrefab;
    public ComponentPool<CreatureView> creaturePoolPrefab;
    public ComponentPool<ZoneView> zonePoolPrefab;
    public ComponentPool<Cell3DView> cellPoolPrefab;
    public ComponentPool<OpponentView> opponentPoolPrefab;

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

        Container.Bind<IComponentPool<ZoneView>>()
            .To<ZonePool>()
            .FromComponentInNewPrefab(zonePoolPrefab)
            .AsSingle();

        Container.Bind<IComponentPool<CreatureView>>()
            .To<CreaturePool>()
            .FromComponentInNewPrefab(creaturePoolPrefab)
            .AsSingle();

        Container.Bind<IComponentPool<Cell3DView>>()
            .To<CellPool>()
            .FromComponentInNewPrefab(cellPoolPrefab)
            .AsSingle();

        Container.Bind<IComponentPool<OpponentView>>()
            .To<OpponentPool>()
            .FromComponentInNewPrefab(opponentPoolPrefab)
            .AsSingle();
    }
}