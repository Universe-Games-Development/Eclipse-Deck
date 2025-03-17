using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "LocationData", menuName = "Map/Location")]
public class LocationData : ScriptableObject {
    public LocationType locationType;
    public string sceneName;
    public AssetLabelReference assetLabel;
    public string displayName;
    [TextArea] public string description;
    public Sprite previewImage;
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

