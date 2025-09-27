using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using UnityEngine;

public class CreatureView : UnitView {
    [SerializeField] MovementComponent movementComponent;
    private void Awake() {
        if (movementComponent == null) {
            Debug.LogWarning("Movement component not set");
        }
    }
    public async UniTask DoTweener(Tweener moveTween) {
        await movementComponent.ExecuteTween(moveTween);
    }

    internal void UpdateDisplay(CardDisplayContext context) {
        throw new NotImplementedException();
    }
}