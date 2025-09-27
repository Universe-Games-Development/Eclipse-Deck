using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class CellFactory : MonoBehaviour {
    [SerializeField] private Cell3DView cellPrefab;
    
    public Cell3DView CreateCell(Cell cell) {
        if (cellPrefab == null) {
            Debug.LogError("Cell prefab is not assigned in CellFactory.");
            return null;
        }
        
        Cell3DView cellView = Instantiate(cellPrefab);

        //if (!cellView.TryGetComponent(out ZonePresenter zonePresenter)) {
        //    zonePresenter = cellView.AddComponent<ZonePresenter>();
        //}
        //zonePresenter.Initialize(cellView, cell);
        return cellView;
    }
}
