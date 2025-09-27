
// This is not Boiler plate! elements will have specific animations
using DG.Tweening;
using UnityEngine;

public class CostDisplayComponent : SingleDisplayComponent {
    private int _previousCost;

    public override void UpdateDisplay(CardDisplayContext context) {
        int newCost = context.Data.cost;
        text.text = newCost.ToString();

        // Специфічна анімація для зміни вартості
        if (newCost != _previousCost) {
            AnimateCostChange(newCost, _previousCost);
            _previousCost = newCost;
        }

        SetVisibility(context.Config.showCost);
    }

    private void AnimateCostChange(int newCost, int oldCost) {
        if (newCost < oldCost) {
            // Анімація зменшення вартості - зелене світіння
            text.DOColor(Color.green, 0.3f).OnComplete(() => text.DOColor(Color.white, 0.3f));
            PulseIcon();
        } else {
            // Анімація збільшення вартості - червоне світіння
            text.DOColor(Color.red, 0.3f).OnComplete(() => text.DOColor(Color.white, 0.3f));
            icon.DOShakePosition(0.3f, 0.1f);
        }
    }
}
