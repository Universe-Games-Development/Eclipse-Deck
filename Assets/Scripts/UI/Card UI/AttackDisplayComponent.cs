using DG.Tweening;
using UnityEngine;

public class AttackDisplayComponent : SingleDisplayComponent {
    private int _previousAttack;

    public override void UpdateDisplay(CardDisplayContext context) {
        int newAttack = context.Data.attack;
        text.text = newAttack.ToString();

        if (newAttack != _previousAttack) {
            AnimateAttackChange(newAttack, _previousAttack);
            _previousAttack = newAttack;
        }

        SetVisibility(context.Config.showStats);
    }

    private void AnimateAttackChange(int newAttack, int oldAttack) {
        if (newAttack > oldAttack) {
            // Анімація підвищення атаки - різке збільшення
            transform.DOScale(1.3f, 0.1f).OnComplete(() => transform.DOScale(1f, 0.2f));
            text.transform.DOPunchRotation(Vector3.forward * 30f, 0.3f);
        }
    }
}