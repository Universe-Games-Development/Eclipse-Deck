using DG.Tweening;
using TMPro;
using UnityEngine;

public abstract class CardDisplayComponent : MonoBehaviour {
    public abstract void UpdateDisplay(CardDisplayContext context);
}

public abstract class SingleDisplayComponent : CardDisplayComponent {
    [SerializeField] protected TextMeshPro text;
    [SerializeField] protected Transform icon;

    public void SetVisibility(bool visible) {
        if (icon != null)
            icon.gameObject.SetActive(visible);
        if (text != null)
            text.gameObject.SetActive(visible);
    }

    // Загальні методи для анімацій
    protected void PulseIcon() {
        icon.DOScale(1.2f, 0.2f).SetLoops(2, LoopType.Yoyo);
    }

    protected void HighlightText() {
        text.color = Color.yellow;
        text.DOColor(Color.white, 0.5f);
    }
}