public interface IGridLayout {
    LayoutResult Calculate(Grid<ItemLayoutInfo> gridData);
}

public interface ILinearLayout {
    LayoutResult Calculate(ItemLayoutInfo[] row);
}