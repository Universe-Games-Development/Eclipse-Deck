using UnityEngine;

public class PlayerInitializer : OpponentInitializer {
    protected override OpponentPresenter CreatePresenter(Opponent opponent, OpponentView view) {
        if (view is not PlayerView playerView) {
            Debug.LogError("Invalid view type for PlayerPresenter");
            return null;
        }

        OpponentPresenter opponentPresenter  = presenterFactory.CreateUnitPresenter<PlayerPresenter>(opponent, playerView);

        PlayerSelectorService playerSelectorService = container.Instantiate<PlayerSelectorService>(new object[] { playerView.SelectionDisplay });
        targetFiller.RegisterSelector(opponent.OwnerId, playerSelectorService);

        return opponentPresenter;
    }
}
