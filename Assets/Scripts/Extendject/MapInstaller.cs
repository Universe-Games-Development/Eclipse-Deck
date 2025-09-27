using Zenject;

public class MapInstaller : MonoInstaller<MapInstaller> {
    public override void InstallBindings() {
        Container.Bind<RoomActivityManager>().AsSingle().NonLazy();

        
    }
}