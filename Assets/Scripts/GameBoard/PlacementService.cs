using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// ������������ ������ ���������� �������� �� ������� �����
/// </summary>
//public class PlacementService<T> : IDisposable where T : class {
//    private readonly Board _board;

//    private readonly Dictionary<Area, T> _areaToObject = new();
//    private readonly Dictionary<T, Area> _objectToArea = new();

//    public event EventHandler<ObjectsDisplacedEvent<T>> ObjectsDisplaced;

//    public PlacementService(Board gameBoard) {
//        _board = gameBoard ?? throw new ArgumentNullException(nameof(gameBoard));

//        SubscribeToBoardEvents();
//    }

//    #region Event Handling
//    private void SubscribeToBoardEvents() {
//        _board.CellSizeChanged += OnCellSizeChanged;
//        _board.ColumnRemoved += OnCellRemoved;
//    }

//    private void UnsubscribeFromBoardEvents() {
//        _board.CellSizeChanged -= OnCellSizeChanged;
//        _board.ColumnRemoved -= OnCellRemoved;
//    }

//    /// <summary>
//    /// ���������� ������� ��������� ������� �������
//    /// </summary>
//    private void OnCellSizeChanged(object sender, CellSizeChangedEvent e) {
//        if (e.NewMaxAreas >= e.OldMaxAreas) return;

//        string message = $"Cell ({e.RowIndex}, {e.CellIndex}) size reduced from {e.OldMaxAreas} to {e.NewMaxAreas}";
//        HandleDisplacedObjects(e.RemovedAreas, message);
//    }

//    /// <summary>
//    /// ���������� ������� �������� �������
//    /// </summary>
//    private void OnCellRemoved(object sender, ColumnRemovedEvent @event) {
//        var removedAreas = @event.RemovedColumn.SelectMany(c => c.Areas);
//        string message = $"Cell {@event.OldCellIndex} was removed";
//        HandleDisplacedObjects(removedAreas, message);
//    }

//    private void HandleDisplacedObjects(IEnumerable<Area> areasToRemove, string eventMessage) {
//        Dictionary<T, Area> displacedObjectsWithAreas = new();

//        foreach (var removedArea in areasToRemove) {
//            if (_areaToObject.TryGetValue(removedArea, out var obj)) {
//                // ������� �� ����� �������� ���������
//                RemoveFromMaps(obj, removedArea);
//                displacedObjectsWithAreas.Add(obj, removedArea);
//            }
//        }

//        if (displacedObjectsWithAreas.Keys.Count > 0) {
//            ObjectsDisplacedEvent<T> objectsDisplacedEvent = new ObjectsDisplacedEvent<T>(displacedObjectsWithAreas, eventMessage);
//            ObjectsDisplaced?.Invoke(this, objectsDisplacedEvent);
//        }
//    }

//    #endregion

//    #region Private Helper Methods
//    /// <summary>
//    /// ��������� ������ � ��� �������� ���������
//    /// </summary>
//    private void AddToMaps(Area area, T obj) {
//        _areaToObject[area] = obj;
//        _objectToArea[obj] = area;
//    }

//    /// <summary>
//    /// ������� ������ �� ����� �������� ���������
//    /// </summary>
//    private bool RemoveFromMaps(T obj, Area area) {
//        bool removedFromAreaMap = _areaToObject.Remove(area);
//        bool removedFromObjectMap = _objectToArea.Remove(obj);

//        // ��������� ���������������
//        if (removedFromAreaMap != removedFromObjectMap) {
//            throw new InvalidOperationException("Maps inconsistency detected during removal");
//        }

//        return removedFromAreaMap;
//    }

//    /// <summary>
//    /// ������� ������ �� ����� �������� �� �������
//    /// </summary>
//    private bool RemoveFromMapsByArea(Area area) {
//        if (_areaToObject.TryGetValue(area, out var obj)) {
//            return RemoveFromMaps(obj, area);
//        }
//        return false;
//    }

//    /// <summary>
//    /// ������� ������ �� ����� �������� �� �������
//    /// </summary>
//    private bool RemoveFromMapsByObject(T obj) {
//        if (_objectToArea.TryGetValue(obj, out var area)) {
//            return RemoveFromMaps(obj, area);
//        }
//        return false;
//    }
//    #endregion

//    /// <summary>
//    /// ��������� ������ � ���������� �������
//    /// </summary>
//    public OperationResult TryPlaceObject(int row, int col, int areaIndex, T obj) {
//        var area = _board.GetArea(row, col, areaIndex);

//        return TryPlaceObject(area, obj);
//    }

//    public OperationResult TryPlaceObject(Area area, T obj) {
//        if (obj == null)
//            return OperationResult.Failed("Object is null");

//        if (area == null)
//            return OperationResult.Failed("Area not found");

//        if (IsAreaOccupied(area))
//            return OperationResult.Failed("Area already occupied");

//        if (IsObjectPlaced(obj))
//            return OperationResult.Failed("Object already placed");

//        AddToMaps(area, obj);
//        return OperationResult.Success();
//    }

//    /// <summary>
//    /// ��������� ������ � ������ ��������� ������� �������
//    /// </summary>
//    public OperationResult TryPlaceObjectInCell(int row, int col, T obj) {
//        var Cell = _board.GetCell(row, col);

//        return TryPlaceObjectInCell(Cell, obj);
//    }

//    public OperationResult TryPlaceObjectInCell(Cell Cell, T obj) {
//        if (obj == null)
//            return OperationResult.Failed("Object is null");

//        if (Cell == null)
//            return OperationResult.Failed("Cell not found");

//        if (IsObjectPlaced(obj))
//            return OperationResult.Failed("Object already placed");

//        var freeArea = GetFreeCellArea(Cell);
//        if (freeArea == null)
//            return OperationResult.Failed("No free area in Cell");

//        AddToMaps(freeArea, obj);
//        return OperationResult.Success();
//    }

//    /// <summary>
//    /// �������� �������������� ������� - ������ O(1)
//    /// </summary>
//    public Area GetObjectLocation(T obj) {
//        if (obj == null) return null;
//        _objectToArea.TryGetValue(obj, out var area);
//        return area;
//    }

//    /// <summary>
//    /// ���������, �������� �� ������ �� ����� - ������ O(1)
//    /// </summary>
//    public bool IsObjectPlaced(T obj) {
//        if (obj == null) return false;
//        return _objectToArea.ContainsKey(obj);
//    }

//    /// <summary>
//    /// �������� ������ ��������� ������� � �������
//    /// </summary>
//    public Area GetFreeCellArea(int row, int col) {
//        var Cell = _board.GetCell(row, col);
//        return GetFreeCellArea(Cell);
//    }

//    /// <summary>
//    /// �������� ������ ��������� ������� � �������
//    /// </summary>
//    public Area GetFreeCellArea(Cell Cell) {
//        if (Cell == null) return null;
//        return Cell.Areas.FirstOrDefault(area => !IsAreaOccupied(area));
//    }

//    /// <summary>
//    /// ������� ������ � ����� - ������ O(1)
//    /// </summary>
//    public bool RemoveObject(T obj) {
//        if (obj == null) return false;
//        return RemoveFromMapsByObject(obj);
//    }

//    /// <summary>
//    /// ������� ������ �� ������� - ������ O(1)
//    /// </summary>
//    public T RemoveObjectFromArea(Area area) {
//        if (area == null) return null;

//        if (_areaToObject.TryGetValue(area, out var obj)) {
//            RemoveFromMapsByArea(area);
//            return obj;
//        }

//        return null;
//    }

//    /// <summary>
//    /// ������� ��� ������� �� �������
//    /// </summary>
//    public List<T> RemoveObjectsFromCell(int row, int col) {
//        var Cell = _board.GetCell(row, col);
//        if (Cell == null) return new List<T>();

//        var objectsToRemove = GetObjectsInCell(Cell);
//        foreach (var obj in objectsToRemove) {
//            RemoveObject(obj);
//        }

//        return objectsToRemove;
//    }

//    /// <summary>
//    /// ���������, ������ �� ������� - O(1)
//    /// </summary>
//    public bool IsAreaOccupied(Area area) {
//        if (area == null) return false;
//        return _areaToObject.ContainsKey(area);
//    }

//    /// <summary>
//    /// ���������, ������ �� �������
//    /// </summary>
//    public bool IsAreaOccupied(int row, int col, int areaIndex) {
//        var area = _board.GetArea(row, col, areaIndex);
//        return IsAreaOccupied(area);
//    }

//    /// <summary>
//    /// ���������, ����� �� �������
//    /// </summary>
//    public bool IsAreaEmpty(Area area) {
//        return !IsAreaOccupied(area);
//    }

//    /// <summary>
//    /// ���������, ��������� �� �������
//    /// </summary>
//    public bool IsCellFull(int row, int col) {
//        var Cell = _board.GetCell(row, col);
//        return IsCellFull(Cell);
//    }

//    /// <summary>
//    /// ���������, ��������� �� �������
//    /// </summary>
//    public bool IsCellFull(Cell Cell) {
//        if (Cell == null) return false;
//        return GetObjectsInCell(Cell).Count >= Cell.MaxAreas;
//    }

//    /// <summary>
//    /// ���������, ����� �� �������
//    /// </summary>
//    public bool IsCellEmpty(int row, int col) {
//        var Cell = _board.GetCell(row, col);
//        if (Cell == null) return true;
//        return GetObjectsInCell(Cell).Count == 0;
//    }

//    /// <summary>
//    /// �������� ���������� ��������� �������� � �������
//    /// </summary>
//    public int GetFreeAreasInCell(int row, int col) {
//        var Cell = _board.GetCell(row, col);
//        if (Cell == null) return 0;
//        return Cell.MaxAreas - GetObjectsInCell(Cell).Count;
//    }

//    /// <summary>
//    /// �������� ��� ������� �� �����
//    /// </summary>
//    public List<T> GetAllObjects() {
//        return _objectToArea.Keys.ToList();
//    }

//    /// <summary>
//    /// �������� ��� ������� � ����
//    /// </summary>
//    public List<T> GetAllObjectsInRow(int rowIndex) {
//        var boardRow = _board.GetRow(rowIndex);
//        if (boardRow == null) return new List<T>();

//        var objects = new List<T>();
//        foreach (var Cell in boardRow.Cells) {
//            foreach (var area in Cell.Areas) {
//                if (_areaToObject.TryGetValue(area, out var obj)) {
//                    objects.Add(obj);
//                }
//            }
//        }
//        return objects;
//    }

//    /// <summary>
//    /// �������� ������� � �������
//    /// </summary>
//    public List<T> GetObjectsInCell(int row, int col) {
//        var Cell = _board.GetCell(row, col);
//        return GetObjectsInCell(Cell);
//    }

//    /// <summary>
//    /// �������� ������� � �������
//    /// </summary>
//    public List<T> GetObjectsInCell(Cell Cell) {
//        if (Cell == null) return new List<T>();

//        var objects = new List<T>();
//        foreach (var area in Cell.Areas) {
//            if (_areaToObject.TryGetValue(area, out var obj)) {
//                objects.Add(obj);
//            }
//        }
//        return objects;
//    }

//    /// <summary>
//    /// �������� ��������� ������������� ��������� ����������
//    /// </summary>
//    public string GetPlacementState() {
//        var sb = new StringBuilder();
//        for (int r = 0; r < _board.RowCount; r++) {
//            var row = _board.GetRow(r);
//            sb.Append($"Row {r}: ");
//            foreach (var Cell in row.Cells) {
//                sb.Append($"Col {Cell.Index} [");
//                for (int a = 0; a < Cell.MaxAreas; a++) {
//                    var area = Cell.GetArea(a);
//                    if (_areaToObject.TryGetValue(area, out var obj)) {
//                        sb.Append(obj.ToString());
//                    } else {
//                        sb.Append("Empty");
//                    }
//                    if (a < Cell.MaxAreas - 1) sb.Append(", ");
//                }
//                sb.Append("] | ");
//            }
//            sb.AppendLine();
//        }
//        return sb.ToString();
//    }

//    /// <summary>
//    /// �������� �������������� �������� (��� ����������)
//    /// </summary>
//    public bool ValidateConsistency() {
//        if (_areaToObject.Count != _objectToArea.Count) {
//            return false;
//        }

//        foreach (var kvp in _areaToObject) {
//            if (!_objectToArea.TryGetValue(kvp.Value, out var area) || area != kvp.Key) {
//                return false;
//            }
//        }

//        foreach (var kvp in _objectToArea) {
//            if (!_areaToObject.TryGetValue(kvp.Value, out var obj) || obj != kvp.Key) {
//                return false;
//            }
//        }

//        return true;
//    }

//    /// <summary>
//    /// ����������� ������� � ������������ �� �������
//    /// </summary>
//    public void Dispose() {
//        UnsubscribeFromBoardEvents();
//        _areaToObject.Clear();
//        _objectToArea.Clear();
//    }
//}

//public struct ObjectsDisplacedEvent<T> : IEvent where T : class {
//    public string Reason { get; }

//    public Dictionary<T, Area> DisplacedObjectsWithAreas { get; }

//    public ObjectsDisplacedEvent(Dictionary<T, Area> displacedObjectsWithAreas, string reason = null) {
//        DisplacedObjectsWithAreas = displacedObjectsWithAreas ?? new();
//        Reason = reason ?? "";
//    }
//}
