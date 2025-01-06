using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewReward", menuName = "Rewards/Reward")]
public abstract class RewardSO : ScriptableObject {
    public abstract void ApplyReward(Opponent opponent); // јбстрактний метод дл€ застосуванн€ нагороди
}

[CreateAssetMenu(fileName = "NewCardReward", menuName = "Rewards/CardReward")]
public class CardRewardSO : RewardSO {
    public List<CardSO> cards;

    public override void ApplyReward(Opponent opponent) {
        Debug.Log("card to pick");
    }
}

[CreateAssetMenu(fileName = "NewHealthReward", menuName = "Rewards/HealthReward")]
public class HealthRewardSO : RewardSO {
    public int healAmount;

    public override void ApplyReward(Opponent opponent) {
        opponent.health.Heal(healAmount);
    }
}

[CreateAssetMenu(fileName = "NewHealthReward", menuName = "Rewards/MaxHealthReward")]
public class MaxHealthRewardSO : RewardSO {
    public int healthIncrease;

    public override void ApplyReward(Opponent opponent) {
        opponent.health.SetMaxValue(healthIncrease);
    }
}