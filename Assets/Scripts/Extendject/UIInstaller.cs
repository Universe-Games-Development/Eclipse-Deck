using Zenject;

public class UIInstaller : MonoInstaller<UIInstaller> {
    public override void InstallBindings() {
        Container.Bind<IDungeonUIService>().To<DungeonMapUIController>().FromComponentInHierarchy().AsSingle();
        Container.Bind<DialogueSystem>().FromComponentInHierarchy().AsSingle();


        Container.Bind<CardTextureRenderer>().FromComponentInHierarchy().AsSingle();
        // Можливо, сюди варто додати й інші UI-сервіси та контролери
    }
}
