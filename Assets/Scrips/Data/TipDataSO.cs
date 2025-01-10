using UnityEngine;

[CreateAssetMenu(fileName = "TipData", menuName = "ScriptableObjects/TipData")]
public class TipDataSO : ScriptableObject {
    [TextArea] public string tipText;
}
