using UnityEngine;
using Zenject;

public class DungeonRunner : MonoBehaviour {
    [Inject] TravelManager travelManager;
    public void Start() {
        travelManager.BeginRun();
    }
}

