using Zenject;

public class EntitiesInstaller : MonoInstaller<EntitiesInstaller> {
    public override void InstallBindings() {
        Container.Bind<RoomSystem>().FromComponentInHierarchy().AsSingle();
        Container.Bind<IUnitRegistry>().To<UnitRegistry>().AsSingle().NonLazy();

        Container.Bind<IOperationFactory>().To<OperationFactory>().AsSingle();
        Container.Bind<IVisualTaskFactory>().To<VisualTaskFactory>().AsSingle();

        Container.Bind(typeof(IUnitSpawner<,,>))
         .To(typeof(UnitSpawner<,,>))
         .AsTransient();

        Container.Bind<IPresenterFactory>().To<PresenterFactory>().AsSingle(); 

        Container.Bind<IOpponentFactory>().To<OpponentFactory>().AsSingle();
        Container.Bind<IDeckBuilder>().To<DeckBuilder>().AsSingle();
        
        Container.Bind<ICardFactory>().To<CardFactory>().AsSingle();
        Container.Bind<IEntityFactory>().To<EntityFactory>().AsSingle();
    }
}
