using UnityEngine;
using Zenject;

public class OpponentInitializer : MonoBehaviour {
    [Inject] protected readonly IOpponentFactory opponentFactory;
    [Inject] protected readonly IComponentPool<OpponentView> opponentPool;
    [Inject] protected readonly IPresenterFactory presenterFactory;
    [Inject] protected readonly IUnitRegistry unitRegistry;
    [Inject] protected ITargetFiller targetFiller;
    [Inject] protected DiContainer container;

    // Створює ворога та його presenter
    public OpponentPresenter CreateOpponent(OpponentData data, OpponentView opponentView) {
        if (data == null) {
            Debug.LogWarning("OpponentData is null");
            return null;
        }

        Opponent opponent = opponentFactory.CreateOpponent(data);

        HandPresenter handPresenter = presenterFactory.CreateUnitPresenter<HandPresenter>(opponentView.HandDisplay, opponent.Hand);
        DeckPresenter deckPresenter = presenterFactory.CreateUnitPresenter<DeckPresenter>(opponentView.DeckDisplay, opponent.Deck);
        unitRegistry.Register(handPresenter);
        unitRegistry.Register(deckPresenter);

        OpponentPresenter opponentPresenter = CreatePresenter(opponent, opponentView);
        unitRegistry.Register(opponentPresenter);
        return opponentPresenter;
    }

    protected virtual OpponentPresenter CreatePresenter(Opponent opponent, OpponentView view) {
        return presenterFactory.CreateUnitPresenter<OpponentPresenter>(opponent, view);
    }
}