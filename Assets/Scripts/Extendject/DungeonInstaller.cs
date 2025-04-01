using Zenject;

public class DungeonInstaller : MonoInstaller<DungeonInstaller> {
    public override void InstallBindings() {
        Container.Bind<IDungeonGenerator>().To<DungeonGenerator>().AsSingle();
        Container.Bind<DungeonVisualizer>().FromComponentInHierarchy().AsCached().Lazy();
        Container.Bind<EnemySpawner>().FromComponentInHierarchy().AsSingle();
    }
}