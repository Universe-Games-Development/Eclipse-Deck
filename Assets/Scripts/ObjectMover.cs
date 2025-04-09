using Cysharp.Threading.Tasks;
using UnityEngine;

public class ObjectMover : MonoBehaviour
{
    [SerializeField] Transform movable;
    private void Awake() {
        if (movable == null) {
            Debug.Log("movable is null taking origin...");
            movable = transform;
        }
    }
    
}
