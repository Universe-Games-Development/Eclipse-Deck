using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class CellFactory : MonoBehaviour {
    [SerializeField] private Cell3DView cellPrefab;
    
    public Cell3DView CreateCell() {
        if (cellPrefab == null) {
            Debug.LogError("Cell prefab is not assigned in CellFactory.");
            return null;
        }
        
        return Instantiate(cellPrefab);
    }
}
