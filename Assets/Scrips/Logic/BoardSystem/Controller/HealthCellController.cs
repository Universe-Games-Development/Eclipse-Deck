using System;
using UnityEngine;

public class HealthCellController : MonoBehaviour {
    private Health health;
    internal void AssignOwner(Opponent opponent) {
        health = opponent.Health;
        
    }
}