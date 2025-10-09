using UnityEngine;
using Zenject;

public class BoardGame : MonoBehaviour
{
    [Inject] IPresenterFactory presenterFactory;
    [Inject] IOpponentFactory opponentFactory;
    [Inject] IOpponentRegistry opponentRegistry;
    [Inject] DiContainer container;
    [Inject] ITargetFiller targetFiller;
    [SerializeField] public OpponentRegistry OpponentsRepresentation;
    [SerializeField] PlayerData playerData;

    [SerializeField] CardHandView handView;
    [SerializeField] OpponentView opponentView;

    public PlayerSelectorService SelectorService;
    
    public SelectorView SelectionDisplay;

    private Opponent player;
    private void Awake() {
        TestInit();
    }

    private void TestInit() {
        player = opponentFactory.CreatePlayer(playerData);

        opponentRegistry.RegisterOpponent(player);
        PlayerPresenter playerPresenter = presenterFactory.CreateUnitPresenter<PlayerPresenter>(player, opponentView);
        playerPresenter.Initialize();

        SelectorService = container.Instantiate<PlayerSelectorService>(new object[] { SelectionDisplay });
        targetFiller.RegisterSelector(player.Id, SelectorService);

        container.Instantiate<PlayerController>(new object[] { player, playerPresenter, SelectorService });
    }

    public void OnDestroy() {
        if (player != null)
        targetFiller.UnregisterSelector(player.Id);
    }
}


