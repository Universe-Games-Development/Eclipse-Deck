using Zenject;

public class EntitiesInstaller : MonoInstaller<EntitiesInstaller> {
    public override void InstallBindings() {
        Container.Bind<RoomSystem>().FromComponentInHierarchy().AsSingle();
    }
}
