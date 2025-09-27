using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

public class GlobalInstaller : MonoInstaller {
    [SerializeField] private List<GameObject> managerPrefabs;

    [SerializeField] private LocationsData locationsData;
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

        // Resourses
        Container.Bind<LocationTransitionManager>().AsSingle().WithArguments(locationsData).NonLazy();
        Container.Bind<ResourceLoadingManager>().AsSingle();
        Container.Bind<VisitedLocationsService>().AsSingle();

        Container.Bind<IEventBus<IEvent>>().To<GameEventBus>().AsSingle().NonLazy();
        Container.Bind<CommandManager>().AsSingle().NonLazy();

        Container.Bind<CardProvider>().AsSingle();
        Container.Bind<EnemyResourceProvider>().AsSingle();

        Container.Bind<EnemyResourceLoader>().AsSingle();
        Container.Bind<CardResourceLoader>().AsSingle();

        Container.BindInstance(_randomConfig).AsSingle();

        Container.Bind<IRandomService>().To<RandomService>().AsSingle().NonLazy();
    }
}

[Serializable]
public class RandomConfig {
    public string Seed = "I like onions!";
    public bool UseRandomSeed = false;
}

public interface IRandomService {
    int Seed { get; }
    System.Random SystemRandom { get; }
}

public class RandomService : IRandomService {
    public int Seed { get; }
    public System.Random SystemRandom { get; }

    public RandomService(RandomConfig config) {
        Seed = CalculateSeed(config);

        SystemRandom = new System.Random(Seed);
        UnityEngine.Random.InitState(Seed);

        UnityEngine.Debug.Log($"Initialized RandomService with Seed: {Seed}");
    }

    private int CalculateSeed(RandomConfig config) {
        if (config.UseRandomSeed) {
            return Guid.NewGuid().GetHashCode();
        } else {
            return GetStableHashCode(config.Seed);
        }
    }

    private int GetStableHashCode(string str) {
        unchecked {
            int hash1 = 5381;
            int hash2 = hash1;

            for (int i = 0; i < str.Length && str[i] != '\0'; i += 2) {
                hash1 = ((hash1 << 5) + hash1) ^ str[i];
                if (i == str.Length - 1 || str[i + 1] == '\0')
                    break;
                hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
            }

            return hash1 + (hash2 * 1566083941);
        }
    }
}