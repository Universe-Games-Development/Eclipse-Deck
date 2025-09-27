using System;
using UnityEngine;

public class OpponentView : UnitView {
    public HealthCellView HealthDisplay;
    public CardHandView HandDisplay;
    public DeckView DeckDisplay;

    public void UpdateHealth(float health, float maxHealth) {
        HealthDisplay.UpdateHealth(health, maxHealth);
    }
}


