using UnityEngine;
using Zenject;

public class PlayerInitializer : MonoBehaviour {
    [Inject] IPresenterFactory presenterFactory;
    [Inject] IOpponentFactory opponentFactory;
    
    [Inject] DiContainer container;
    [Inject] ITargetFiller targetFiller;

    [SerializeField] PlayerData playerData;
    [SerializeField] OpponentView opponentView;
    public SelectorView SelectionDisplay;

    [SerializeField] OpponentData opponentData;
    public void CreatePlayer() {
        Opponent player = opponentFactory.CreatePlayer(playerData);

        PlayerSelectorService SelectorService = CreateSelectorUI();

        PlayerPresenter playerPresenter = presenterFactory.CreateUnitPresenter<PlayerPresenter>(player, opponentView, SelectorService);
        playerPresenter.Initialize();

        targetFiller.RegisterSelector(player.InstanceId, SelectorService);
        
    }

    private PlayerSelectorService CreateSelectorUI() {
        return container.Instantiate<PlayerSelectorService>(new object[] { SelectionDisplay });
    }
}