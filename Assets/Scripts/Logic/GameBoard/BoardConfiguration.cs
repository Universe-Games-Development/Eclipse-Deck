using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Конфигурационный класс для настройки доски
/// </summary>
public class BoardConfiguration {
    private readonly List<List<int>> _rowConfigurations;
    private int? _fixedColumnCount = null;

    public IReadOnlyList<IReadOnlyList<int>> RowConfigurations =>
        _rowConfigurations.Select(row => row.AsReadOnly() as IReadOnlyList<int>).ToList();

    public int RowCount => _rowConfigurations.Count;
    public int ColumnCount => _fixedColumnCount ?? 0;

    public BoardConfiguration() {
        _rowConfigurations = new List<List<int>>();
    }

    public BoardConfiguration AddRow(params int[] columnAreas) {
        if (columnAreas == null || columnAreas.Length == 0)
            throw new ArgumentException("Ряд должен содержать минимум одну колонку");

        if (columnAreas.Any(areas => areas < 1))
            throw new ArgumentException("Каждая колонка должна иметь минимум одну площадь");

        if (_fixedColumnCount.HasValue) {
            if (columnAreas.Length != _fixedColumnCount.Value)
                throw new ArgumentException($"Количество колонок должно быть {_fixedColumnCount.Value}, получено {columnAreas.Length}");
        } else {
            _fixedColumnCount = columnAreas.Length;
        }

        _rowConfigurations.Add(new List<int>(columnAreas));
        return this;
    }

    public void Validate() {
        if (_rowConfigurations.Count < 2)
            throw new InvalidOperationException("Доска должна иметь минимум два ряда");

        if (!_fixedColumnCount.HasValue || _fixedColumnCount.Value < 1)
            throw new InvalidOperationException("Доска должна иметь минимум одну колонку");

        if (_rowConfigurations.Any(row => row.Count != _fixedColumnCount.Value))
            throw new InvalidOperationException("Все ряды должны иметь одинаковое количество колонок");
    }
}
