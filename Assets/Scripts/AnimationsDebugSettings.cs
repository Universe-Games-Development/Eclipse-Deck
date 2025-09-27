using UnityEngine;

public class AnimationsDebugSettings : MonoBehaviour {
    [SerializeField] private bool _skipAllAnimations = false;

    public bool SkipAllAnimations {
        get => _skipAllAnimations;
        set => _skipAllAnimations = value;
    }
    public void ToggleAnimations() {
        _skipAllAnimations = !_skipAllAnimations;
        Debug.Log($"Animations are now {(_skipAllAnimations ? "DISABLED" : "ENABLED")}");
    }
}
