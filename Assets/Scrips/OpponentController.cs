using UnityEngine;

public class OpponentController : MonoBehaviour, ILogicHolder<Opponent> {
    public Opponent Logic { get; private set; }
}