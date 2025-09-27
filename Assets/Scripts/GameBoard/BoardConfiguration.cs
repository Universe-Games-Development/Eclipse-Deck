using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// ���������������� ����� ��� ��������� �����
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
            throw new ArgumentException("��� ������ ��������� ������� ���� �������");

        if (columnAreas.Any(areas => areas < 1))
            throw new ArgumentException("������ ������� ������ ����� ������� ���� �������");

        if (_fixedColumnCount.HasValue) {
            if (columnAreas.Length != _fixedColumnCount.Value)
                throw new ArgumentException($"���������� ������� ������ ���� {_fixedColumnCount.Value}, �������� {columnAreas.Length}");
        } else {
            _fixedColumnCount = columnAreas.Length;
        }

        _rowConfigurations.Add(new List<int>(columnAreas));
        return this;
    }

    public void Validate() {
        if (_rowConfigurations.Count < 2)
            throw new InvalidOperationException("����� ������ ����� ������� ��� ����");

        if (!_fixedColumnCount.HasValue || _fixedColumnCount.Value < 1)
            throw new InvalidOperationException("����� ������ ����� ������� ���� �������");

        if (_rowConfigurations.Any(row => row.Count != _fixedColumnCount.Value))
            throw new InvalidOperationException("��� ���� ������ ����� ���������� ���������� �������");
    }
}
