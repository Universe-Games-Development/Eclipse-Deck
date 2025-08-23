using TMPro;
using UnityEngine;

public class Zone3DView : MonoBehaviour {
    [SerializeField] TextMeshPro text;
    public void UpdateSummonedCount(int count) {
        text.text = $"Units: {count}";
    }
}
