using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;


public abstract class GenericResourceLoader<T> : IResourceLoader where T : class {
    protected Dictionary<AssetLabelReference, List<T>> _loadedResources = new();
    protected Dictionary<AssetLabelReference, AsyncOperationHandle<IList<T>>> _handles = new();
    protected Dictionary<AssetLabelReference, UniTaskCompletionSource<List<T>>> _loadingTasks = new();

    protected ResourceLoadingManager _resourceLoadingManager;

    public abstract int LoadPriority { get; }

    protected GenericResourceLoader(ResourceLoadingManager resourceLoadingManager) {
        _resourceLoadingManager = resourceLoadingManager;
        _resourceLoadingManager.RegisterResourceLoader(this);
    }

    /// <summary>
    /// ��������� ������� �� �������� �����������.
    /// </summary>
    public async UniTask LoadResources(AssetLabelReference assetLabel, IProgress<float> progress = null) {
        if (assetLabel == null) {
            Debug.LogWarning("Trying to load empty asset label");
            return;
        }

        // ���� ������� ��� ����������, ������ ��������
        if (_loadedResources.ContainsKey(assetLabel)) {
            progress?.Report(1f);
            return;
        }

        // �������� ������� ����� �������� �����
        try {
            await GetResourcesForLocationAsync(assetLabel, default, progress);
        } catch (Exception e) {
            Debug.LogError($"Failed to load resources for {assetLabel}: {e}");
            throw;
        }
    }

    /// <summary>
    /// ������ ������� ��� ��������� ����������. ���� ������� �� �� ����������,
    /// ��������� �� ���������� � ������� ���������.
    /// </summary>
    public async UniTask<List<T>> GetResourcesForLocationAsync(
        AssetLabelReference assetLabel,
        CancellationToken cancellationToken = default,
        IProgress<float> progress = null
    ) {
        // ���������� ����� ���
        if (assetLabel == null) {
            Debug.LogWarning("Asset label is null");
            return new List<T>();
        }

        // ����������, �� � ������� ��� ����������
        if (_loadedResources.TryGetValue(assetLabel, out var cachedResources)) {
            progress?.Report(1f);
            return cachedResources;
        }

        // ����������, �� ��� � ������� ������������
        UniTaskCompletionSource<List<T>> loadingTask;
        bool isNew = false;

        lock (_loadingTasks) {
            if (_loadingTasks.TryGetValue(assetLabel, out loadingTask)) {
                // ���� ������������ ��� � ������, ������ ���� ����������
            } else {
                // ��������� ���� �������� ������������
                loadingTask = new UniTaskCompletionSource<List<T>>();
                _loadingTasks[assetLabel] = loadingTask;
                isNew = true;
            }
        }

        // ���� ������� ������ ���� ������������
        if (isNew) {
            try {
                // ��������� ������������ � �������� �������
                var resources = await LoadAssetsAsync(assetLabel, progress);

                // �������� ��������� � ���
                _loadedResources[assetLabel] = resources;

                // ���������� ��� ���������� ������������
                loadingTask.TrySetResult(resources);

                return resources;
            } catch (Exception e) {
                // � ��� ������� ����������� ��� ��
                loadingTask.TrySetException(e);
                throw;
            } finally {
                // ��������� �������� � ��������
                lock (_loadingTasks) {
                    _loadingTasks.Remove(assetLabel);
                }
            }
        } else {
            // ������ �� ��������� ������������ � ��������� ����������
            try {
                return await loadingTask.Task.AttachExternalCancellation(cancellationToken);
            } catch (OperationCanceledException) {
                Debug.LogWarning($"Operation canceled for asset: {assetLabel}");
                throw;
            }
        }
    }

    /// <summary>
    /// ��������� ������ �� ��������� Addressables
    /// </summary>
    private async UniTask<List<T>> LoadAssetsAsync(AssetLabelReference assetLabel, IProgress<float> progress) {
        AsyncOperationHandle<IList<T>> handle = default;
        try {
            // ��������� ������������
            handle = Addressables.LoadAssetsAsync<T>(assetLabel, null);

            // ������ �� ����������
            await handle.Task;

            // ����������� ��� �������
            progress?.Report(1f);

            // �������� �����
            _handles[assetLabel] = handle;

            // ��������� ���������
            return handle.Result.ToList();
        } catch (Exception e) {
            Debug.LogError($"Error loading assets for {assetLabel}: {e}");
            if (handle.IsValid()) {
                Addressables.Release(handle);
            }
            throw;
        }
    }

    /// <summary>
    /// ��������� ����� ��� ��������� �������. �� ���� �� ������������.
    /// ������� ������� ����� ���� ���� ��� ����������.
    /// </summary>
    public List<T> GetResourcesForLocation(AssetLabelReference assetLabel) {
        return _loadedResources.TryGetValue(assetLabel, out var resources)
            ? resources
            : new List<T>();
    }

    /// <summary>
    /// ��������, �� ���������� ������� ��� ���� �������
    /// </summary>
    public bool HasResources(AssetLabelReference assetLabel) {
        return _loadedResources.ContainsKey(assetLabel);
    }

    /// <summary>
    /// ��������, �� � ������ ������������ ������� ��� ���� �������
    /// </summary>
    public bool IsLoadingLocation(AssetLabelReference assetLabel) {
        return _loadingTasks.ContainsKey(assetLabel);
    }

    /// <summary>
    /// ��������� ������� ��� ������� �������
    /// </summary>
    public virtual void UnloadByLocation(AssetLabelReference assetLabel) {
        if (_handles.TryGetValue(assetLabel, out var handle)) {
            Addressables.Release(handle);
            _handles.Remove(assetLabel);
            _loadedResources.Remove(assetLabel);
        }
    }

    /// <summary>
    /// ��������� �� ���������� �������
    /// </summary>
    public void UnloadAll() {
        foreach (var assetLabel in _loadedResources.Keys.ToList()) {
            UnloadByLocation(assetLabel);
        }
        _loadedResources.Clear();
        _handles.Clear();
    }

    /// <summary>
    /// ������� �� ���������� �������
    /// </summary>
    public List<T> GetAllResources() {
        return _loadedResources.Values
            .Where(list => list != null && list.Count > 0)
            .SelectMany(list => list)
            .ToList();
    }
}

public class CardResourceLoader : GenericResourceLoader<CardData> {
    public CardResourceLoader(ResourceLoadingManager resourceLoadingManager) : base(resourceLoadingManager) {
    }

    public override int LoadPriority => 1;
}

public class EnemyResourceLoader : GenericResourceLoader<EnemyData> {
    public EnemyResourceLoader(ResourceLoadingManager resourceLoadingManager) : base(resourceLoadingManager) { }

    public override int LoadPriority => 2;
}

// Gives interface to get location recources
public interface IResourceProvider<T> where T : class {
    List<T> GetResources(AssetLabelReference assetLabel);
    T GetRandomResource(AssetLabelReference assetLabel);
}

public abstract class GenericResourceProvider<T> : IResourceProvider<T> where T : class {
    protected readonly GenericResourceLoader<T> _loader;

    // ������������� ��'����� ����� �����������
    public GenericResourceProvider(GenericResourceLoader<T> loader) {
        _loader = loader;
    }

    public List<T> GetResources(AssetLabelReference assetLabel) =>
        _loader.GetResourcesForLocation(assetLabel);

    public T GetRandomResource(AssetLabelReference assetLabel) {
        var resources = GetResources(assetLabel);
        return resources.Count > 0 ? resources[UnityEngine.Random.Range(0, resources.Count)] : null;
    }
}

public class CardProvider : GenericResourceProvider<CardData> {
    private readonly Dictionary<Type, List<CardData>> _cardsByType = new();
    private readonly List<CardData> _unlockedCards = new();

    public CardProvider(
        CardResourceLoader loader
    ) : base(loader) {
    }

    public void UpdateAvailableCards(List<CardData> rewardSets) {
        _unlockedCards.Clear();
        _unlockedCards.AddRange(rewardSets);
        // ��������� ��� �� ������
        RefreshCardTypeCache(_unlockedCards);
    }

    private void RefreshCardTypeCache(List<CardData> cards) {
        _cardsByType.Clear();
        foreach (var card in cards) {
            var cardType = card.GetType();
            if (!_cardsByType.TryGetValue(cardType, out var typeList)) {
                typeList = new List<CardData>();
                _cardsByType[cardType] = typeList;
            }
            typeList.Add(card);
        }
    }

    // ������������ ���� �� ������� � ����������
    public List<CardData> GetCardsForLocation(LocationData locationData) {
        return _loader.GetResourcesForLocation(locationData.assetLabel);
    }

    public List<CardData> GetUnlockedCards() {
        return _loader.GetAllResources();
    }

    public List<CardData> GetRandomUnlockedCards(int count) {
        if (count <= 0) return new List<CardData>();

        List<CardData> cardDatas = GetUnlockedCards();
        return cardDatas.OrderBy(_ => UnityEngine.Random.value).Take(count).ToList();
    }
}

public class EnemyResourceProvider : GenericResourceProvider<EnemyData> {
    private EnemiesLocationCache enemiesLocationCache = new();

    private LocationTransitionManager _transitionManager;

    public EnemyResourceProvider(EnemyResourceLoader loader, LocationTransitionManager transitionManager
    ) : base(loader) {
        _transitionManager = transitionManager;
    }

    public async UniTask<List<EnemyData>> GetEnemiesAsync(EnemyType requestEnemyType) {
        if (_transitionManager.GetSceneLocation() == null) {
            Debug.LogWarning("Current location data is null");
            return new List<EnemyData>();
        }
        
        if (TryGetCachedEnemies(requestEnemyType, out var cachedEnemies)) {
            return cachedEnemies;
        }

        AssetLabelReference assetLabel = _transitionManager.GetSceneLocation().assetLabel;
        List<EnemyData> allEnemies = await _loader.GetResourcesForLocationAsync(assetLabel);
        enemiesLocationCache.Store(assetLabel, allEnemies);

        return enemiesLocationCache.TryGetFromCache(requestEnemyType, assetLabel, out var newlyCachedEnemies)
            ? newlyCachedEnemies
            : new List<EnemyData>();
    }

    public bool TryGetCachedEnemies(EnemyType requestEnemyType, out List<EnemyData> enemyDatas) {
        if (_transitionManager.GetSceneLocation() == null) {
            Debug.LogWarning("Current location data is null");
            enemyDatas = new();
            return false;
        }
        AssetLabelReference assetLabel = _transitionManager.GetSceneLocation().assetLabel;

        return enemiesLocationCache.TryGetFromCache(requestEnemyType, assetLabel, out enemyDatas);
    }

    public List<EnemyData> GetEnemiesByLabel(AssetLabelReference assetLabel) {
        if (enemiesLocationCache.TryGetFromCache(assetLabel, out var enemies)) {
            return enemies;
        }
        return new List<EnemyData>();
    }


    public void ClearCache() {
        enemiesLocationCache = new EnemiesLocationCache();
    }

    public class EnemiesLocationCache {
        private readonly Dictionary<AssetLabelReference, Dictionary<EnemyType, List<EnemyData>>> _cacheByLabel = new();

        public bool TryGetFromCache(EnemyType type, AssetLabelReference label, out List<EnemyData> enemies) {
            enemies = null;
            if (_cacheByLabel.TryGetValue(label, out var byType)) {
                return byType.TryGetValue(type, out enemies);
            }
            return false;
        }

        public void Store(AssetLabelReference label, List<EnemyData> allEnemiesForLocation) {
            if (!_cacheByLabel.ContainsKey(label)) {
                _cacheByLabel[label] = new Dictionary<EnemyType, List<EnemyData>>();
            }

            var typeDict = _cacheByLabel[label];

            var groupedByType = allEnemiesForLocation.GroupBy(e => e.enemyType);
            foreach (var group in groupedByType) {
                typeDict[group.Key] = group.ToList();
            }
        }

        public bool TryGetFromCache(AssetLabelReference label, out List<EnemyData> enemies) {
            enemies = null;
            if (_cacheByLabel.TryGetValue(label, out var byType)) {
                enemies = byType.Values.SelectMany(list => list).ToList();
                return true;
            }
            return false;
        }
        public void Clear() {
            _cacheByLabel.Clear();
        }

        public void ClearLocation(AssetLabelReference label) {
            _cacheByLabel.Remove(label);
        }
    }
}