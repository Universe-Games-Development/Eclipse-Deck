using UnityEngine;

public class EnemyInitializer : OpponentInitializer {
    protected override OpponentPresenter CreatePresenter(Opponent opponent, OpponentView view) {
        OpponentPresenter opponentPresenter  = presenterFactory.CreateUnitPresenter<EnemyPresenter>(opponent, view);
        container.Instantiate<AIController>(new object[] { opponent });
        return opponentPresenter;
    }
}