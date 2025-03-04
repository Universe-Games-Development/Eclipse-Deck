using UnityEngine;
using Zenject;
using System.Collections.Generic;

public class GlobalInstaller : MonoInstaller {
    [SerializeField] private List<GameObject> managerPrefabs;

    public override void InstallBindings() {
        foreach (var prefab in managerPrefabs) {
            var component = prefab.GetComponent<MonoBehaviour>();
            if (component != null) {
                Container.Bind(component.GetType()).FromComponentInNewPrefab(prefab).AsSingle().NonLazy();
            } else {
                Debug.LogWarning($"Prefab {prefab.name} does not have a component.");
            }
        }

        // Resourses
        Container.Bind<GameEventBus>().AsSingle().NonLazy();
        Container.Bind<CommandManager>().AsSingle().NonLazy();
        Container.Bind<CardManager>().AsSingle().NonLazy();
        Container.Bind<AddressablesResourceManager>().AsSingle().NonLazy();
    }
}
