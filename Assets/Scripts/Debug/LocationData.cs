using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "LocationData", menuName = "Map/Location")]
public class LocationData : ScriptableObject {
    public AssetLabelReference assetLabel;
    public SceneReference sceneReference;
    public Sprite previewImage;
    [TextArea] public string description;
    public bool isPlayableLevel = true;
    public int orderInSequence;

    public MapGenerationData mapGenerationData;

    public LocationRoomsData locationRoomsData; // Contains all rooms views for this location
    public ActivitiesData activitiesData;
    internal LocationType locationType;

    public string GetName() {
        return sceneReference.SceneName;
    }
}
