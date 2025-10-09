using Cysharp.Threading.Tasks;
using ModestTree;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;


// Please dont watch this class under the influence of alcohol
public class OpponentPresenter : UnitPresenter, IDisposable {
    
    [Inject] public IOperationManager operationManager;
    [Inject] public IEventBus<IEvent> eventBus;
    

    [Inject] protected readonly IPresenterFactory presenterFactory;
    [Inject] public IUnitRegistry unitRegistry;

    public Opponent Opponent { get; private set; }
    public OpponentView OpponentView;

    public HandPresenter HandPresenter;
    public DeckPresenter DeckPresenter;

    public OpponentPresenter(Opponent opponent, OpponentView opponentView) : base(opponent, opponentView) {
        Opponent = opponent;
        OpponentView = opponentView;
    }

    #region Initialization

    public virtual void Initialize() {
        HandPresenter = presenterFactory.CreateUnitPresenter<HandPresenter>(Opponent.Hand, OpponentView.HandDisplay);
        DeckPresenter = presenterFactory.CreateUnitPresenter<DeckPresenter>(Opponent.Deck, OpponentView.DeckDisplay);
        DeckPresenter.FillDeckWithRandomCards(20);
    }

    #endregion

    #region Card Management
    
    public bool DrawCards(int drawAmount) {
        if (drawAmount == 0) return true;

        List<Card> drawnCards = DeckPresenter.DrawCards(drawAmount);

        foreach(var card in drawnCards) {
            HandPresenter.AddCard(card);
        }
        
        return drawnCards.Count > 0;
    }
    #endregion

    #region API
    public void DrawTestCards() {
        DrawCards(5);
    }

    public override string ToString() {
        if (Opponent != null) {
            return Opponent.Data.Name;
        }
        return "Opponent";
    }


    #endregion

    public virtual void Dispose() {
    }
}

public class PlayerPresenter : OpponentPresenter, IInputService {
    public PlayerView PlayerView;
    
    public event Action<Card> OnCardSelected;
    public event Action OnEndTurnClicked;

    public PlayerPresenter(Opponent opponent, PlayerView playerView) : base(opponent, playerView) {
        PlayerView = playerView;
    }

    public override void Initialize() {
        base.Initialize();
        
        Opponent.OnCardPlayStarted += OnCardPlayStarted;
        Opponent.OnCardPlayFinished += OnCardPlayFinished;
        HandPresenter.OnHandCardClicked += HandleHandCardSelected;

        
    }

    private void HandleHandCardSelected(CardPresenter presenter) {
        OnCardSelected?.Invoke(presenter.Card);
    }

    private void OnCardPlayFinished(Card card, CardPlayResult result) {
        if (!result.IsSuccess ) {
            HandPresenter.SetInteractiveCard(card, true);
        }
        
        HandPresenter.UpdateCardsOrder();
        HandPresenter.SetInteractable(true);
    }

    private void OnCardPlayStarted(Card card) {
        HandPresenter.SetInteractiveCard(card, false);
        HandPresenter.SetInteractable(false);
    } 

    public override void Dispose() {
        base.Dispose();

        Opponent.OnCardPlayStarted -= OnCardPlayStarted;
        Opponent.OnCardPlayFinished -= OnCardPlayFinished;
        HandPresenter.OnHandCardClicked -= HandleHandCardSelected;
    }
}


