using DG.Tweening;
using UnityEngine;

public class HealthDisplayComponent : SingleDisplayComponent {
    private int _previousHealth;

    public override void UpdateDisplay(CardDisplayContext context) {
        int newHealth = context.Data.health;
        text.text = newHealth.ToString();

        if (newHealth != _previousHealth) {
            AnimateHealthChange(newHealth, _previousHealth);
            _previousHealth = newHealth;
        }

        SetVisibility(context.Config.showStats);
    }

    private void AnimateHealthChange(int newHealth, int oldHealth) {
        if (newHealth > oldHealth) {
            // Анімація лікування - плавне пульсування
            transform.DOPunchScale(Vector3.one * 0.2f, 0.4f);
            text.DOColor(Color.green, 0.2f).OnComplete(() => text.DOColor(Color.white, 0.5f));
        } else {
            // Анімація отримання шкоди - тремтіння
            transform.DOShakePosition(0.3f, 0.1f);
            text.DOColor(Color.red, 0.1f).SetLoops(3, LoopType.Yoyo);
        }
    }
}
