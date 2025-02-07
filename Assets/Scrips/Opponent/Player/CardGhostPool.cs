using UnityEngine;

public class CardGhostPool : BasePool<CardLayoutGhost> {
    public CardGhostPool(CardLayoutGhost ghostPrefab, Transform defaultParent)
        : base(ghostPrefab, defaultParent) {
    }
}