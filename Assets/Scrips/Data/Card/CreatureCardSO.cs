using UnityEngine;

[CreateAssetMenu(fileName = "CreatureCard", menuName = "TGE/Cards/CreatureCard")]
public class CreatureCardSO : CardSO {
    [Header ("Creature Data")]
    public int MAX_CARD_ATTACK = 100;
    public int MAX_CARD_HEALTH = 100;

    public int Attack;
    public int Health;
}
