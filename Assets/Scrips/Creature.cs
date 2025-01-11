using System;
using UnityEngine;

public class Creature {
    public Health Health;
    public Attack attack;
    private Card card;

    public Creature(Card myCard) {
        card = myCard;
    }
}
