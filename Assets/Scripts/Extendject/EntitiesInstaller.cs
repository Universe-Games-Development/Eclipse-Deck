﻿using Zenject;

public class EntitiesInstaller : MonoInstaller<EntitiesInstaller> {
    public override void InstallBindings() {
        Container.Bind<RoomPresenter>().FromComponentInHierarchy().AsSingle();
        Container.Bind<PlayerPresenter>().FromComponentInHierarchy().AsSingle().NonLazy();
        Container.Bind<EnemyPresenter>().FromComponentInHierarchy().AsSingle();
        Container.Bind<AnimationsDebugSettings>().FromComponentInHierarchy().AsSingle();
        Container.Bind<IActionFiller>().To<ActionInputSystem>().FromComponentInHierarchy().AsSingle().WhenInjectedInto<Player>();
        Container.Bind<IActionFiller>().To<EnemyInputSystem>().AsTransient().WhenInjectedInto<Enemy>();
    }
}
