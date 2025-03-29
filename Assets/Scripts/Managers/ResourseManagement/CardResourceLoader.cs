using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

// Loads resources for a location
public interface IResourceLoader {
    // Returns loading progress
    UniTask LoadResources(LocationData locationData, IProgress<float> progress = null);
    bool HasLocationData(LocationData locationData);

    int LoadPriority { get; }
}
public abstract class GenericResourceLoader<T> : IResourceLoader where T : class {
    protected Dictionary<LocationType, List<T>> _loadedResources = new();
    protected Dictionary<LocationType, AsyncOperationHandle<IList<T>>> _handles = new();
    protected LocationTransitionManager _transitionManager;
    public abstract int LoadPriority { get; }

    protected GenericResourceLoader(LocationTransitionManager transitionManager) {
        _transitionManager = transitionManager;
        transitionManager.RegisterResourceLoader(this);
    }

    public async UniTask LoadResources(LocationData locationData, IProgress<float> progress = null) {
        if (locationData == null) {
            Debug.LogWarning("Trying to load empty data");
            return;
        }
        LocationType location = locationData.locationType;

        if (_handles.ContainsKey(location)) {
            Debug.LogWarning($"Resources already loaded for: {location}");
            return;
        }

        AsyncOperationHandle<IList<T>> handle = default;

        try {
            handle = await LoadAssetsForLocation(locationData, progress);

            _loadedResources[location] = handle.Result.ToList();
            _handles[location] = handle;

        } catch (Exception e) {
            Debug.LogError($"Failed to load resources: {e}");
            if (handle.IsValid()) {
                Addressables.Release(handle);
            }
            throw;
        }
    }

    protected virtual async UniTask<AsyncOperationHandle<IList<T>>> LoadAssetsForLocation(LocationData locationData, IProgress<float> progress) {
        AsyncOperationHandle<IList<T>> handle = Addressables.LoadAssetsAsync<T>(locationData.assetLabel, null);
        await handle.Task;
        progress?.Report(1f);
        return handle;
    }

    public bool HasLocationData(LocationData locationData) {
        return _loadedResources.ContainsKey(locationData.locationType);
    }

    public List<T> GetResourcesForLocation(LocationType location) {
        return _loadedResources.TryGetValue(location, out var resources)
            ? resources
            : new List<T>();
    }

    public virtual void UnloadByLocation(LocationType location) {
        if (_handles.TryGetValue(location, out var handle)) {
            Addressables.Release(handle);
            _handles.Remove(location);
            _loadedResources.Remove(location);
        }
    }

    public void UnloadAll() {
        foreach (var location in _loadedResources.Keys.ToList()) {
            UnloadByLocation(location);
        }
        _loadedResources.Clear();
    }

    public List<T> GetAllResources() {
        return _loadedResources.Values
            .Where(list => list != null && list.Count > 0)
            .SelectMany(list => list)
            .ToList();
    }
}
public class CardResourceLoader : GenericResourceLoader<CardData> {
    public CardResourceLoader(LocationTransitionManager transitionManager) : base(transitionManager) {
    }

    public override int LoadPriority => 1;
}
public class EnemyResourceLoader : GenericResourceLoader<OpponentData> {
    private Dictionary<LocationType, List<OpponentData>> _bossesByLocation = new();
    private Dictionary<LocationType, List<OpponentData>> _regularEnemiesByLocation = new();
    public EnemyResourceLoader(LocationTransitionManager transitionManager) : base(transitionManager) {
    }

    public override int LoadPriority => 2;

    protected override async UniTask<AsyncOperationHandle<IList<OpponentData>>> LoadAssetsForLocation(
    LocationData locationData,
    IProgress<float> progress
) {
        var handle = await base.LoadAssetsForLocation(locationData, progress);

        if (handle.Status == AsyncOperationStatus.Succeeded) {
            var enemies = handle.Result;
            _bossesByLocation[locationData.locationType] = enemies.Where(e => e.isBoss).ToList();
            _regularEnemiesByLocation[locationData.locationType] = enemies.Where(e => !e.isBoss).ToList();
        }

        return handle;
    }

    public List<OpponentData> GetBossesForLocation(LocationType location) {
        return _bossesByLocation.TryGetValue(location, out var bosses)
            ? bosses
            : new List<OpponentData>();
    }

    public List<OpponentData> GetRegularEnemiesForLocation(LocationType location) {
        return _regularEnemiesByLocation.TryGetValue(location, out var enemies)
            ? enemies
            : new List<OpponentData>();
    }

    public override void UnloadByLocation(LocationType location) {
        base.UnloadByLocation(location);
        _bossesByLocation.Remove(location);
        _regularEnemiesByLocation.Remove(location);
    }
}

// Gives interface to get location recources
public interface IResourceProvider<T> where T : class {
    List<T> GetResources(LocationType location);
    T GetRandomResource(LocationType location);
}

public abstract class GenericResourceProvider<T> : IResourceProvider<T> where T : class {
    protected readonly GenericResourceLoader<T> _loader;

    // Використовуємо ін'єкцію через конструктор
    public GenericResourceProvider(GenericResourceLoader<T> loader) {
        _loader = loader;
    }

    public List<T> GetResources(LocationType location) =>
        _loader.GetResourcesForLocation(location);

    public T GetRandomResource(LocationType location) {
        var resources = GetResources(location);
        return resources.Count > 0 ? resources[UnityEngine.Random.Range(0, resources.Count)] : null;
    }
}

public class CardProvider : GenericResourceProvider<CardData> {
    private readonly List<CardData> _unclokedCards = new();
    private readonly VisitedLocationsService _visitedLocationsService;

    public CardProvider(
        VisitedLocationsService visitedLocationsService,
        CardResourceLoader loader
    ) : base(loader) {
        _visitedLocationsService = visitedLocationsService;
    }

    public void UpdateAvailableCards(List<CardData> rewardSets) {
        _unclokedCards.Clear();
        _unclokedCards.AddRange(rewardSets);
    }

    public List<CardData> GetUnlockedCards() {
        return _visitedLocationsService.GetVisitedLocations()
            .SelectMany(location => _loader.GetResourcesForLocation(location))
            .ToList();
    }

    public List<CardData> GetRandomUnlockedCards(int count = 0) {
        List<CardData> cardDatas = GetUnlockedCards();
        if (count == 0) return cardDatas;
        return cardDatas.OrderBy(_ => UnityEngine.Random.value).Take(count).ToList();
    }
}

public class EnemyProvider : GenericResourceProvider<OpponentData> {
    private readonly LocationTransitionManager _transitionManager;
    private readonly EnemyResourceLoader _enemyResourceLoader;

    public EnemyProvider(
        EnemyResourceLoader loader,
        LocationTransitionManager transitionManager
    ) : base(loader) {
        _transitionManager = transitionManager;
        _enemyResourceLoader = loader;
    }

    public OpponentData GetEnemyData() {
        var currentLocation = _transitionManager.CurrentLocationData.locationType;
        var enemies = _loader.GetResourcesForLocation(currentLocation);
        return enemies.Count > 0 ? enemies.GetRandomElement() : null;
    }

    public OpponentData GetBossData() {
        var currentLocation = _transitionManager.CurrentLocationData.locationType;
        var bosses = _enemyResourceLoader.GetBossesForLocation(currentLocation);
        return bosses.Count > 0 ? bosses.GetRandomElement() : null;
    }
}
