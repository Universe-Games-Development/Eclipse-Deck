using Zenject;

public class EntitiesInstaller : MonoInstaller<EntitiesInstaller> {
    public override void InstallBindings() {
        Container.Bind<RoomSystem>().FromComponentInHierarchy().AsSingle();
        Container.Bind<IUnitPresenterRegistry>().To<UnitPresenterRegistry>().AsSingle().NonLazy();

        Container.Bind<IOperationFactory>().To<OperationFactory>().AsSingle();
        Container.Bind<IVisualTaskFactory>().To<VisualTaskFactory>().AsSingle();

        Container.Bind<ICardFactory<Card3DView>>().To<CardFactory<Card3DView>>().AsSingle();
        Container.Bind<ICreatureFactory<Card3DView>>().To<CreatureFactory<Card3DView>>().AsSingle();
    }
}
