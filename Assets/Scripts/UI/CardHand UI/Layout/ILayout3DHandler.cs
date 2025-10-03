using UnityEngine;

public interface ILayout3DHandler {
    LayoutResult Calculate(Grid<ItemLayoutInfo> gridData, bool useDefaulSizes = true);
    LayoutResult Calculate(ItemLayoutInfo[] row, bool useDefaulSizes = true);
}
