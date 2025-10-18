using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Універсальний сервис размещения объектов на игровой доске
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
//    /// Обработчик события изменения размера колонки
//    /// </summary>
//    private void OnCellSizeChanged(object sender, CellSizeChangedEvent e) {
//        if (e.NewMaxAreas >= e.OldMaxAreas) return;

//        string message = $"Cell ({e.RowIndex}, {e.CellIndex}) size reduced from {e.OldMaxAreas} to {e.NewMaxAreas}";
//        HandleDisplacedObjects(e.RemovedAreas, message);
//    }

//    /// <summary>
//    /// Обработчик события удаления колонки
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
//                // Удаляем из обоих словників синхронно
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
//    /// Добавляет объект в оба словника синхронно
//    /// </summary>
//    private void AddToMaps(Area area, T obj) {
//        _areaToObject[area] = obj;
//        _objectToArea[obj] = area;
//    }

//    /// <summary>
//    /// Удаляет объект из обоих словників синхронно
//    /// </summary>
//    private bool RemoveFromMaps(T obj, Area area) {
//        bool removedFromAreaMap = _areaToObject.Remove(area);
//        bool removedFromObjectMap = _objectToArea.Remove(obj);

//        // Проверяем консистентность
//        if (removedFromAreaMap != removedFromObjectMap) {
//            throw new InvalidOperationException("Maps inconsistency detected during removal");
//        }

//        return removedFromAreaMap;
//    }

//    /// <summary>
//    /// Удаляет объект из обоих словників по области
//    /// </summary>
//    private bool RemoveFromMapsByArea(Area area) {
//        if (_areaToObject.TryGetValue(area, out var obj)) {
//            return RemoveFromMaps(obj, area);
//        }
//        return false;
//    }

//    /// <summary>
//    /// Удаляет объект из обоих словників по объекту
//    /// </summary>
//    private bool RemoveFromMapsByObject(T obj) {
//        if (_objectToArea.TryGetValue(obj, out var area)) {
//            return RemoveFromMaps(obj, area);
//        }
//        return false;
//    }
//    #endregion

//    /// <summary>
//    /// Размещает объект в конкретной области
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
//    /// Размещает объект в первой свободной области колонки
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
//    /// Получает местоположение объекта - теперь O(1)
//    /// </summary>
//    public Area GetObjectLocation(T obj) {
//        if (obj == null) return null;
//        _objectToArea.TryGetValue(obj, out var area);
//        return area;
//    }

//    /// <summary>
//    /// Проверяет, размещен ли объект на доске - теперь O(1)
//    /// </summary>
//    public bool IsObjectPlaced(T obj) {
//        if (obj == null) return false;
//        return _objectToArea.ContainsKey(obj);
//    }

//    /// <summary>
//    /// Получает первую свободную область в колонке
//    /// </summary>
//    public Area GetFreeCellArea(int row, int col) {
//        var Cell = _board.GetCell(row, col);
//        return GetFreeCellArea(Cell);
//    }

//    /// <summary>
//    /// Получает первую свободную область в колонке
//    /// </summary>
//    public Area GetFreeCellArea(Cell Cell) {
//        if (Cell == null) return null;
//        return Cell.Areas.FirstOrDefault(area => !IsAreaOccupied(area));
//    }

//    /// <summary>
//    /// Удаляет объект с доски - теперь O(1)
//    /// </summary>
//    public bool RemoveObject(T obj) {
//        if (obj == null) return false;
//        return RemoveFromMapsByObject(obj);
//    }

//    /// <summary>
//    /// Удаляет объект из области - теперь O(1)
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
//    /// Удаляет все объекты из колонки
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
//    /// Проверяет, занята ли область - O(1)
//    /// </summary>
//    public bool IsAreaOccupied(Area area) {
//        if (area == null) return false;
//        return _areaToObject.ContainsKey(area);
//    }

//    /// <summary>
//    /// Проверяет, занята ли область
//    /// </summary>
//    public bool IsAreaOccupied(int row, int col, int areaIndex) {
//        var area = _board.GetArea(row, col, areaIndex);
//        return IsAreaOccupied(area);
//    }

//    /// <summary>
//    /// Проверяет, пуста ли область
//    /// </summary>
//    public bool IsAreaEmpty(Area area) {
//        return !IsAreaOccupied(area);
//    }

//    /// <summary>
//    /// Проверяет, заполнена ли колонка
//    /// </summary>
//    public bool IsCellFull(int row, int col) {
//        var Cell = _board.GetCell(row, col);
//        return IsCellFull(Cell);
//    }

//    /// <summary>
//    /// Проверяет, заполнена ли колонка
//    /// </summary>
//    public bool IsCellFull(Cell Cell) {
//        if (Cell == null) return false;
//        return GetObjectsInCell(Cell).Count >= Cell.MaxAreas;
//    }

//    /// <summary>
//    /// Проверяет, пуста ли колонка
//    /// </summary>
//    public bool IsCellEmpty(int row, int col) {
//        var Cell = _board.GetCell(row, col);
//        if (Cell == null) return true;
//        return GetObjectsInCell(Cell).Count == 0;
//    }

//    /// <summary>
//    /// Получает количество свободных областей в колонке
//    /// </summary>
//    public int GetFreeAreasInCell(int row, int col) {
//        var Cell = _board.GetCell(row, col);
//        if (Cell == null) return 0;
//        return Cell.MaxAreas - GetObjectsInCell(Cell).Count;
//    }

//    /// <summary>
//    /// Получает все объекты на доске
//    /// </summary>
//    public List<T> GetAllObjects() {
//        return _objectToArea.Keys.ToList();
//    }

//    /// <summary>
//    /// Получает все объекты в ряду
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
//    /// Получает объекты в колонке
//    /// </summary>
//    public List<T> GetObjectsInCell(int row, int col) {
//        var Cell = _board.GetCell(row, col);
//        return GetObjectsInCell(Cell);
//    }

//    /// <summary>
//    /// Получает объекты в колонке
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
//    /// Получает строковое представление состояния размещения
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
//    /// Валідація консистентності словників (для діагностики)
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
//    /// Освобождает ресурсы и отписывается от событий
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
