using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "LocationData", menuName = "Map/Location")]
public class LocationData : ScriptableObject {
    public LocationType locationType;
    public string displayName;
    public string sceneName;
    public Sprite previewImage;
    [TextArea] public string description;
    public LocationRoomsData locationRoomsData;

    public AssetLabelReference assetLabel;
    
    public bool isPlayableLevel = true;
    public int orderInSequence;

    private void OnValidate() {
        if (string.IsNullOrEmpty(sceneName)) {
            sceneName = locationType.ToString();
        }
        if (string.IsNullOrEmpty(displayName)) {
            displayName = locationType.ToString();
        }
    }
}

