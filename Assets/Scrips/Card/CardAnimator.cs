using DG.Tweening;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class CardAnimator : MonoBehaviour {
    public Action OnReachedLayout;

    private bool isHovered;
    private Tween hoveringTween;
    private CardUI cardUI;
    private int originalSiblingIndex;

    private Vector3 originalScale;
    public CardLayoutGhost CardLayoutGhost { get; internal set; }

    [Header("Layout")]
    [SerializeField] private RectTransform globalBody;
    [SerializeField] private RectTransform innerBody;

    public void AttachAnimator(CardUI cardUI) {
        cardUI.OnCardClicked += ShrinkClick;
        originalSiblingIndex = cardUI.transform.GetSiblingIndex();
    }

    private void ShrinkClick(CardUI uI) {
        FlyByLayout();
    }

    public void FlyByLayout() {
        Vector3 rectNewPosition = CardLayoutGhost.RectTransform.position;
        Vector3 newPosition = CardLayoutGhost.transform.position;

        Vector3 oldPosition = globalBody.transform.position;
        globalBody.transform.DOMove(CardLayoutGhost.RectTransform.position, 0.8f).SetEase(Ease.InOutSine);
    }

    internal async Task FlyAwayWithCallback() {
        globalBody.transform.DOLocalMoveY(globalBody.transform.position.y - 2f, 0.8f).SetEase(Ease.InOutSine);
    }

    public void FlyTo(Vector3 targetWorldPosition) {
        // Конвертуємо світові координати в локальні координати для батьківського об'єкта
        Vector3 localTarget = globalBody.parent.InverseTransformPoint(targetWorldPosition);

        Vector3 target = new Vector3(0, 5, 0);
        // Анімуємо локальні координати
        
    }


    internal void Reset() {
        throw new NotImplementedException();
    }

    internal void ToggleHover(bool value) {
        bool hovered = value;
    }

    private void Awake() {
        originalScale = innerBody.localScale;
    }


    private void OnDestroy() {
        if (cardUI)
        cardUI.OnCardClicked -= ShrinkClick;
    }

    private void OnDrawGizmos() {
        if (CardLayoutGhost && globalBody) {
            Gizmos.DrawSphere(CardLayoutGhost.transform.position, 0.05f);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(globalBody.transform.position, 0.05f);

            Gizmos.DrawLine(globalBody.transform.position, CardLayoutGhost.transform.position);
        }
    }
}