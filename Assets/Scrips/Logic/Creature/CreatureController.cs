using System;
using UnityEngine;

public class CreatureController : MonoBehaviour {
    public CreatureCard LinkedCard { get; internal set; }
    [SerializeField] Transform modelParent;

    public void Initialize(CreatureCard creatureCard) {
        LinkedCard = creatureCard;
        Instantiate(creatureCard.creatureCardData, modelParent);
    }

    public void MoveTo(Field field) {
        gameObject.transform.position = field.GetCoordinates();
    }

    internal void SetView(CreatureView view) {
        CreatureView viewObject = Instantiate(view, modelParent);
    }
}
