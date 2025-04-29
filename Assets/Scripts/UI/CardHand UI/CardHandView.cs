using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public interface ICardHandView {
    event Action<CardView> CardClicked;

    void Cleanup();
    CardView CreateCardView();
    void DeselectCardView(CardView cardView);
    void RemoveCardUI(CardView cardView);
    void SelectCardView(CardView cardView);
    void SetInteractable(bool value);
}
// VIEW - відповідає лише за відображення і повідомляє про взаємодію через події
public class CardHandView : MonoBehaviour, ICardHandView {
    public event Action<CardView> CardClicked;

    [SerializeField] private Transform cardSpawnPoint;
    [SerializeField] private Transform ghostLayoutParent;
    [SerializeField] private CardView cardPrefab;
    [SerializeField] private CardLayoutGhost ghostPrefab;
    private CancellationTokenSource updatePositionCts;
    private bool isInteractable = true;
    private Dictionary<CardView, CardLayoutGhost> _viewsToGhosts = new();

    public void Start() {
        UpdateCardsPositionsAsync().Forget();
    }

    public CardView CreateCardView() {
        CardView cardView = Instantiate(cardPrefab, cardSpawnPoint);
        CardLayoutGhost ghost = Instantiate(ghostPrefab, ghostLayoutParent);
        _viewsToGhosts.Add(cardView, ghost);

        cardView.SetInteractable(isInteractable);

        // Підписуємося на подію кліку
        cardView.OnCardClicked += OnCardUIClicked;
        UpdateCardsPositionsAsync().Forget();
        return cardView;
    }

    public void RemoveCardUI(CardView cardView) {
        if (_viewsToGhosts.TryGetValue(cardView, out CardLayoutGhost ghost)) {
            _viewsToGhosts.Remove(cardView);
            Destroy(ghost.gameObject);
        }
        cardView.RemoveCardView().Forget();
        UpdateCardsPositionsAsync().Forget();
    }

    private async UniTask UpdateCardsPositionsAsync(int delayFrames = 3) {
        updatePositionCts?.Cancel();

        var newCts = new CancellationTokenSource();
        updatePositionCts = newCts;

        // Strange deal with Unity to wait ghost layout update
        await UniTask.DelayFrame(delayFrames, cancellationToken: updatePositionCts.Token);

        try {
            foreach (var viewGhost in _viewsToGhosts) {
                if (newCts.IsCancellationRequested) return;
                await UniTask.NextFrame(newCts.Token);

                CardView cardView = viewGhost.Key;
                CardLayoutGhost cardLayoutGhost = viewGhost.Value;

                Vector3 newLocalPosition = cardView.transform.parent.InverseTransformPoint(cardLayoutGhost.transform.position);

                cardView.SetInteractable(false);
                cardView.transform.DOLocalMove(newLocalPosition, 0.8f)
                    .SetEase(Ease.InOutSine)
                    .OnComplete(() => {
                        {
                            cardView.SetInteractable(isInteractable);
                        }
                    });
            }
        } catch (OperationCanceledException) {
            Debug.Log("UpdateCardsPositionsAsync cancelled.");
        } finally {
            if (updatePositionCts == newCts) {
                updatePositionCts.Dispose();
                updatePositionCts = null;
            }
        }
    }
    private void OnCardUIClicked(CardView cardView) {
        CardClicked?.Invoke(cardView);
    }
    public void SetInteractable(bool value) {
        isInteractable = value;
        foreach (var cardView in _viewsToGhosts.Keys) {
            cardView.SetInteractable(value);
        }
    }

    public void Cleanup() {
        // Відписуємося від подій
        foreach (var cardView in _viewsToGhosts.Keys) {
            cardView.OnCardClicked -= OnCardUIClicked;
        }

        updatePositionCts?.Cancel();
        updatePositionCts?.Dispose();
    }

    private void OnDestroy() {
        Cleanup();
    }

    public void SelectCardView(CardView cardView) {
        // Dotween animation for selected card moving up
    }

    public void DeselectCardView(CardView cardView) {
        // Dotween animation for selected card moving down
    }
}

// PRESENTER - виступає як повноцінний посередник між Model і View
public class HandPresenter : IDisposable {
    public CardHand CardHand { get; private set; }
    private readonly ICardHandView handView;
    public CardPresenterRegistry CardPresenterRegistry;
    public Action<Card> OnCardSelected;
    public HandPresenter(CardHand cardHand, CardHandView handView) {
        CardPresenterRegistry = new();
        CardHand = cardHand ?? throw new ArgumentNullException(nameof(cardHand));
        this.handView = handView ?? throw new ArgumentNullException(nameof(handView));

        // Підписуємося на події моделі
        cardHand.CardAdded += OnCardAdded;
        cardHand.CardRemoved += OnCardRemoved;
        cardHand.OnCardSelected += SelectCard;
        cardHand.OnCardDeselected += DeselectCard;
        cardHand.OnToggled += handView.SetInteractable;
        // Підписуємося на події представлення
        handView.CardClicked += OnCardUIClicked;

        // Початкова синхронізація
        SyncViewWithModel();
    }
    private void SyncViewWithModel() {
        // Синхронізуємо представлення з поточним станом моделі
        foreach (var card in CardHand.Cards) {
            OnCardAdded(card);
        }

        // Встановлюємо вибрану карту, якщо вона є
        if (CardHand.SelectedCard != null) {
            SelectCard(CardHand.SelectedCard);
        }
    }

    // Handle Model events
    private void OnCardAdded(Card card) {
        // Показуємо карту у представленні
        CardView cardView = handView.CreateCardView();

        CardPresenter cardPresenter = new CardPresenter(card, cardView);
        CardPresenterRegistry.Register(cardPresenter);
    }

    private void OnCardRemoved(Card card) {
        CardPresenter cardPresenter = CardPresenterRegistry.GetByCard(card);
        CardPresenterRegistry.Unregister(cardPresenter);
        handView.RemoveCardUI(cardPresenter.View);
    }

    private void SelectCard(Card card) {
        CardPresenter cardPresenter = CardPresenterRegistry.GetByCard(card);
        CardView cardView = cardPresenter.View;
        handView.SelectCardView(cardView);
        OnCardSelected?.Invoke(card);
    }

    private void DeselectCard(Card card) {
        CardPresenter cardPresenter = CardPresenterRegistry.GetByCard(card);
        CardView cardView = cardPresenter.View;
        handView.DeselectCardView(cardView);
    }

    // Handle View events

    private void OnCardUIClicked(CardView cardView) {
        CardPresenter cardPresenter = CardPresenterRegistry.GetByView(cardView);
        Card card = cardPresenter.Model;
        CardHand.SelectCard(card);
    }

    public void Dispose() {
        // Відписуємося від подій
        CardHand.CardAdded -= OnCardAdded;
        CardHand.CardRemoved -= OnCardRemoved;

        handView.CardClicked -= OnCardUIClicked;
        handView.Cleanup();
    }

    internal void ClearHand() {
        throw new NotImplementedException();
    }
}
public class CardPresenterRegistry {
    private readonly Dictionary<Card, CardPresenter> _byCard = new();
    private readonly Dictionary<CardView, CardPresenter> _byView = new();

    public void Register(CardPresenter presenter) {
        _byCard[presenter.Model] = presenter;
        _byView[presenter.View] = presenter;
    }

    public void Unregister(CardPresenter presenter) {
        _byCard.Remove(presenter.Model);
        _byView.Remove(presenter.View);
    }

    public CardPresenter GetByCard(Card card) {
        if (!_byCard.TryGetValue(card, out CardPresenter cardPresenter)) {
            Debug.LogWarning($"Card {card.Data.Name} not found in registry.");
            return null;
        }
        return cardPresenter;
    }
    public CardPresenter GetByView(CardView view) {
        if (!_byView.TryGetValue(view, out CardPresenter cardPresenter)) {
            Debug.LogWarning($"CardView not found in registry.");
            return null;
        }
        return cardPresenter;
    }

    public IEnumerable<CardView> GetViews() {
        return _byView.Keys;
    }
}



