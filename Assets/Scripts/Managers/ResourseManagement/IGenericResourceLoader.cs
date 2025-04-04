using Cysharp.Threading.Tasks;
using System;
using UnityEngine.AddressableAssets;

public interface IResourceLoader {
    int LoadPriority { get; }
    bool HasResources(AssetLabelReference assetLabel);
    bool IsLoadingLocation(AssetLabelReference assetLabel);
    UniTask LoadResources(AssetLabelReference assetLabel, IProgress<float> progress = null);
    void UnloadAll();
    void UnloadByLocation(AssetLabelReference assetLabel);
}