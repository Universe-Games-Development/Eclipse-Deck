using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class CreatureView : MonoBehaviour {
    [SerializeField] MovementComponent movementComponent;
    private void Awake() {
        if (movementComponent == null) {
            Debug.LogWarning("Movement component not set");
        }
    }
    public void DoTweener(Tweener moveTween) {
        movementComponent.ExecuteTween(moveTween).Forget();
    }
}