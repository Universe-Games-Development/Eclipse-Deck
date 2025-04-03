using UnityEngine;
using Zenject;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;

public class GlobalInstaller : MonoInstaller {
    [SerializeField] private List<GameObject> managerPrefabs;

    [SerializeField] private List<LocationData> _locationDatas;
    [SerializeField] private RandomConfig _randomConfig;
    public override void InstallBindings() {
        foreach (var prefab in managerPrefabs) {
            if (prefab == null) {
                Debug.LogWarning("Manager prefab is null.");
                continue;
            }
            var component = prefab.GetComponent<MonoBehaviour>();
            if (component != null) {
                Container.Bind(component.GetType()).FromComponentInNewPrefab(prefab).AsSingle().NonLazy();
            } else {
                Debug.LogWarning($"Prefab {prefab.name} does not have a component.");
            }
        }
        Container.Bind<InputMapManager>().AsSingle();

        // Resourses
        Container.Bind<LocationTransitionManager>().AsSingle().WithArguments(_locationDatas).NonLazy();
        Container.Bind<ResourceLoadingManager>().AsSingle();
        Container.Bind<VisitedLocationsService>().AsSingle();

        Container.Bind<GameEventBus>().AsSingle().NonLazy();
        Container.Bind<CommandManager>().AsSingle().NonLazy();

        Container.Bind<EnemyManager>().AsSingle();
        Container.Bind<CardProvider>().AsSingle();
        Container.Bind<EnemyResourceLoader>().AsSingle();
        Container.Bind<CardResourceLoader>().AsSingle();

        Container.BindInstance(_randomConfig).AsSingle();
        Container.Bind<IInitializable>().To<RandomSeedInitializer>().AsSingle().NonLazy();
    }
}

[Serializable]
public class RandomConfig {
    public string seed = "I like onions!";
    public bool useRandomSeed = false;
}

public class RandomSeedInitializer : IInitializable {
    private readonly RandomConfig _randomConfig;

    public RandomSeedInitializer(RandomConfig randomConfig) {
        _randomConfig = randomConfig;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        Initialize();
    }

    public void Initialize() {
        int seed = default;
        if (_randomConfig.useRandomSeed) {
            seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        } else {
            seed = _randomConfig.seed.GetHashCode();
        }

        UnityEngine.Random.InitState(seed);
        Debug.Log($"UnityEngine.Random initialized with seed: {seed}");
    }
}