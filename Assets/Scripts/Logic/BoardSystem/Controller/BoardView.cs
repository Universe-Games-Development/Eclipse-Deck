using DG.Tweening;
using System;
using UnityEngine;

public class BoardView : MonoBehaviour {
    [Header("Grid Interaction Params")]
    [Range(0, 10)]
    public float yInteractionRange = 1f;

    private FieldPool pool;
    [Header("Field Data")]
    [SerializeField] private FieldPresenter fieldPrefab;
    [Header("Board Adjuster")]
    [SerializeField] private Transform origin;
    [SerializeField] private Transform globalCenter;

    public void Initialize() {
        if (fieldPrefab == null) throw new ArgumentNullException("field prefab is null");
        pool = new FieldPool(fieldPrefab, origin);
    }

    public FieldPresenter SpawnFieldAt(Vector3 spawnPosition) {
        FieldPresenter fieldPresenter = pool.Get();
        fieldPresenter.transform.localPosition = spawnPosition;
        return fieldPresenter;
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

    internal bool IsWithinYInteractionRange(Vector3 worldPosition) {
        throw new NotImplementedException();
    }
}