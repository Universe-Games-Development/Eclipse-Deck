using UnityEngine;

public class CreatureController : MonoBehaviour, ILogicHolder<Creature> {
    public Creature Logic { get; private set; }
    public CreatureCard LinkedCard { get; internal set; }

    public void Initialize(Creature creature) {
        Logic = creature;
    }
}