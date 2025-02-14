using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class BattleManager {

    [Inject] private OpponentRegistrator Registrator;
    [Inject] GameEventBus eventBus;

    [Inject]
    private void Construct(OpponentRegistrator registrator) {
        Registrator = registrator;
        Registrator.OnOpponentsRegistered += StartBattle;
    }

    public void RegisterOpponentForBattle(Opponent opponent) {
        Registrator.RegisterOpponent(opponent);
    }

    public void StartBattle(List<Opponent> battleOpponents) {
        if (battleOpponents == null || battleOpponents.Count < 2) {
            Debug.LogError("Not enough opponents to start the battle.");
            return;
        }

        if (!Registrator.IsAllRegistered()) {
            Debug.Log("Not all oppponents registered for board battle!");
            return;
        }
        OnBattleBegin battleStartData = new OnBattleBegin(Registrator.GetActiveOpponents());
        eventBus.Raise(battleStartData);
    }

    public void OnBattleEnd() {
        Opponent testWinner = Registrator.GetPlayer();
        Opponent testLooser = Registrator.GetEnemy();
        eventBus.Raise(new BattleEndEventData(testWinner, testLooser));
    }
}
