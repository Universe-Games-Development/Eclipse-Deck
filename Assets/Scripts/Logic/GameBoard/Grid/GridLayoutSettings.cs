using UnityEngine;

[CreateAssetMenu(fileName = "GridLayoutSettings", menuName = "Layouts/Grid")]
public class GridLayoutSettings : ScriptableObject {
    [Header("Linear Settings")]
    public LinearLayoutSettings horizontalSettings; 
    public LinearLayoutSettings verticalSettings;
    public GridAlignmentMode alignmentMode;
}
