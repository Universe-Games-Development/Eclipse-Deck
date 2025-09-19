using DG.Tweening;
using System;
using UnityEngine;
using Zenject;

public class BoardView : MonoBehaviour {
    [Header("Grid Interaction Params")]
    [Range(0, 10)]
    public float yInteractionRange = 1f;

    private FieldPool fieldPool;
    [Header("Field Data")]
    [SerializeField] private FieldView fieldPrefab;
    [Header("Board Adjuster")]
    [SerializeField] private Transform origin;
    [SerializeField] private Transform globalCenter;
    [Inject] IEventBus<IEvent> _eventBus;

    public void Initialize() {
        if (fieldPrefab == null) throw new ArgumentNullException("field prefab is null");
        fieldPool = new FieldPool();
    }

    public FieldView SpawnFieldAt(Vector3 spawnPosition) {
        FieldView fiedlView = fieldPool.Get();
        fiedlView.transform.localPosition = spawnPosition;
        return fiedlView;
    }

    public void MoveTo(Vector3 moveToPosition) {
        origin.DOMove(moveToPosition, 0.5f)
            .SetEase(Ease.InOutSine);
    }

    public Transform GetBoardOrigin() {
        return origin;
    }

    public Transform GetCenter() {
        return globalCenter;
    }

    public Transform GetBoardParent() {
        return origin;
    }

    public bool IsWithinYInteractionRange(Vector3 worldPosition) {
        float boardY = origin.position.y; // The Y position of the board
        float y = worldPosition.y; // The Y position of where the player is pointing

        // Check if the Y position is within the interaction range
        return Math.Abs(y - boardY) <= yInteractionRange;
    }
}