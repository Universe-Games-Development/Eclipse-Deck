using UnityEngine;
using Zenject;

public class BoardGame : MonoBehaviour
{
    [Inject] IPresenterFactory presenterFactory;
    [Inject] IOpponentFactory opponentFactory;
    [SerializeField] public OpponentRegistry OpponentsRepresentation;
    [SerializeField] PlayerData playerData;

    [SerializeField] CardHandView handView;
    [SerializeField] OpponentView opponentView;

    private void Awake() {
        TestInit();
    }

    private void TestInit() {
        Opponent player = opponentFactory.CreatePlayer(playerData);

        PlayerPresenter playerPresenter = presenterFactory.CreateUnitPresenter<PlayerPresenter>(player, opponentView);
        playerPresenter.Initialize();
    }

    public Opponent GetOpponent(Opponent initiator) {
        return OpponentsRepresentation.GetOpponent(initiator);
    }
}


