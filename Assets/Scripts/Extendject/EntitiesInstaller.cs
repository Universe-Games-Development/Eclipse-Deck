using Zenject;

public class EntitiesInstaller : MonoInstaller<EntitiesInstaller> {
    public override void InstallBindings() {
        Container.Bind<RoomSystem>().FromComponentInHierarchy().AsSingle();
        Container.Bind<ICreatureFactory>().To<CreatureFactory>().FromComponentInHierarchy().AsSingle();
        Container.Bind<IUnitPresenterRegistry>().To<UnitPresenterRegistry>().AsSingle().NonLazy();

        Container.Bind<IUnitRegistry>().To<UnitRegistry>().AsSingle().NonLazy();
        Container.Bind<IOperationFactory>().To<OperationFactory>().AsSingle();
        Container.Bind<ICardFactory>().To<CardFactory>().AsSingle();
    }
}
