using UnityEngine;
using Zenject;

public class BoardGame : MonoBehaviour
{
    [Inject] IPresenterFactory presenterFactory;
    [Inject] IOpponentFactory opponentFactory;
    [Inject] IOpponentRegistry opponentRegistry;

    [SerializeField] public OpponentRegistry OpponentsRepresentation;
    [SerializeField] PlayerData playerData;

    [SerializeField] CardHandView handView;
    [SerializeField] OpponentView opponentView;

    private void Awake() {
        TestInit();
    }

    private void TestInit() {
        Opponent player = opponentFactory.CreatePlayer(playerData);
        opponentRegistry.RegisterOpponent(player);
        PlayerPresenter playerPresenter = presenterFactory.CreateUnitPresenter<PlayerPresenter>(player, opponentView);
        playerPresenter.Initialize();
    }

}


