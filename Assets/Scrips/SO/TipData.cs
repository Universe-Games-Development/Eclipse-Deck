using UnityEngine;

[CreateAssetMenu(fileName = "TipData", menuName = "ScriptableObjects/TipData")]
public class TipData : ScriptableObject {
    [TextArea] public string tipText;
}
