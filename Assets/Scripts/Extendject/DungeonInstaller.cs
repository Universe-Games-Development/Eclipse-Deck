using Zenject;

public class DungeonInstaller : MonoInstaller<DungeonInstaller> {
    public override void InstallBindings() {
        Container.Bind<IDungeonGenerator>().To<DungeonGenerator>().AsSingle();
        Container.Bind<DungeonVisualizer>().FromComponentInHierarchy().AsCached().Lazy();
        Container.Bind<RoomPopulator>().AsSingle();

        Container.Bind<EnemyFactory>().FromComponentInHierarchy().AsSingle();
        Container.Bind<IRoomActivityFactory>().To<RoomActivityFactory>().AsSingle();
        Container.Bind<IRoomFactory>().To<RandomRoomFactory>().AsSingle();

    }
}