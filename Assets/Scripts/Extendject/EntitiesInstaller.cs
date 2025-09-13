using Zenject;

public class EntitiesInstaller : MonoInstaller<EntitiesInstaller> {
    public override void InstallBindings() {
        Container.Bind<RoomSystem>().FromComponentInHierarchy().AsSingle();
        Container.Bind<ICreatureSpawnService>().To<CreatureSpawnService>().FromComponentInHierarchy().AsSingle();
        

        Container.Bind<IUnitRegistry>().To<UnitRegistry>().AsSingle().NonLazy();
        Container.Bind<IOperationFactory>().To<OperationFactory>().AsSingle();
        Container.Bind<ICardFactory>().To<CardFactory>().AsSingle();
    }
}
