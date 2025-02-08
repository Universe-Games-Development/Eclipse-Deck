using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using UnityEngine;

public class CardAnimator : MonoBehaviour {
    public Action OnReachedLayout;

    private bool isHovered;
    private Tween hoveringTween;
    private CardUI cardUI;
    private int originalSiblingIndex;
    public CardLayoutGhost CardLayoutGhost { get; internal set; }

    [Header("Layout")]
    [SerializeField] private RectTransform globalBody;
    [SerializeField] private RectTransform innerBody;

    private Tween mainFlyingTween;

    public void AttachAnimator(CardUI cardUI) {
        cardUI.OnLayoutUpdate += FlyByLayout;
        cardUI.OnCardClicked += ShrinkClick;
        cardUI.OnCardHovered += ToggleHover;
        cardUI.OnCardRemoval += RemovalAnimation;
        originalSiblingIndex = cardUI.transform.GetSiblingIndex();
    }

    private void ShrinkClick(CardUI uI) {
        // Create a sequence for the scaling animation
        Sequence shrinkSequence = DOTween.Sequence();
        shrinkSequence.Append(innerBody.DOScale(0.9f, 0.2f));
        shrinkSequence.Append(innerBody.DOScale(1f, 0.2f));
        // Play the sequence
        shrinkSequence.Play();
    }

    private void FlyByLayout() {
        if (mainFlyingTween != null && mainFlyingTween.IsPlaying()) {
            mainFlyingTween.Kill();
        }

        // Отримуємо нову локальну позицію відносно батьківського контейнера
        Vector3 newLocalPosition = globalBody.parent.InverseTransformPoint(CardLayoutGhost.transform.position);

        mainFlyingTween = globalBody.transform.DOLocalMove(newLocalPosition, 0.8f)
            .SetEase(Ease.InOutSine)
            .OnComplete(() => { {
                    mainFlyingTween = null;
                    OnReachedLayout?.Invoke();
                } 
            });
    }


    private async UniTask RemovalAnimation(CardUI cardUI) {
        var sequence = DOTween.Sequence();
        sequence.Append(globalBody.transform.DOScale(Vector3.zero, 0.3f));
        sequence.Join(globalBody.transform.DOLocalMoveY(globalBody.transform.position.y - 2f, 0.8f).SetEase(Ease.InOutSine));
        await sequence.AsyncWaitForCompletion();
    }

    private void ToggleHover(bool value) {
        // Якщо картка вже в потрібному стані (піднята або опущена), ігноруємо повторний виклик
        if (isHovered == value)
            return;

        // Оновлюємо поточний стан
        isHovered = value;
        ToggleSubling(isHovered);

        // Якщо вже є активна анімація hover, припиняємо її
        if (hoveringTween != null && hoveringTween.IsActive())
            hoveringTween.Kill();

        // Обчислюємо цільову позицію по Y.
        float targetY = value ? 300.5f : 0; // Цільова позиція за умовчанням
        hoveringTween = innerBody.DOLocalMoveY(targetY, 0.5f).SetEase(Ease.OutQuad);
    }

    private int lastSublingIndex;

    private void ToggleSubling(bool isHovered) {
        // Збережемо поточний індекс для повернення
        if (isHovered) {
            lastSublingIndex = transform.GetSiblingIndex();
            transform.SetSiblingIndex(transform.parent.childCount - 1); // Перемістити на передній план
        } else {
            // Повернути оригінальний індекс якщо він ще існує
            if (lastSublingIndex < transform.parent.childCount) {
                transform.SetSiblingIndex(lastSublingIndex);
            } else {
                // Якщо початковий індекс більше ніж кількість дітей, ставимо в кінець
                transform.SetSiblingIndex(transform.parent.childCount - 1);
            }
        }
    }
    internal void Reset() {
        Debug.Log("Reset animator card logic");
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