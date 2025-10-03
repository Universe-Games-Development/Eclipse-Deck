using System;
using System.Collections.Generic;
using UnityEngine;

public class BoardView : UnitView {
    [SerializeField] public LayoutSettings layoutSettings;
    [SerializeField] private CellFactory cellFactory;

    public event Action OnUpdateRequest;
    [SerializeField] float updateTime = 1f;
    [SerializeField] bool doUpdate = false;
    private float lastTime;

    Dictionary<(int row, int col), Cell3DView> gridCells = new();
    [SerializeField] public LayoutSettings settings;
    ILayout3DHandler layout;

    protected void Awake() {
    }

    private void Update() {
        if (!doUpdate) return;
        lastTime += Time.deltaTime;
        if (lastTime > updateTime) {
            OnUpdateRequest?.Invoke();
            lastTime = 0;
        }
    }

    public Cell3DView CreateCell(int row, int column) {
        Cell3DView view = cellFactory.CreateCell();
        gridCells.Add((row, column), view);

        view.transform.SetParent(transform);
        return view;
    }

    private void OnDestroy() {
        gridCells.Clear();
    }
}
