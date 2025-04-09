using Zenject;

public class MapInstaller : MonoInstaller<MapInstaller> {
    public override void InstallBindings() {
        Container.Bind<CreatureNavigator>().AsSingle();
        Container.Bind<CreatureBehaviour>().AsTransient();

        Container.Bind<RoomActivityManager>().AsSingle().NonLazy();
        Container.Bind<IRoomActivityFactory>().To<RoomActivityFactory>().AsSingle();
        Container.Bind<IRoomFactory>().To<RandomRoomFactory>().AsSingle();
        Container.Bind<RoomPopulator>().AsSingle();
    }
}