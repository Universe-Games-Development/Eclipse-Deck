using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using Zenject;

public class GameInitializer : MonoBehaviour {
    [SerializeField] BoardGame boardGame;

    [Inject] private LocationTransitionManager _locationManager;
    [Inject] ResourceLoadingManager loadingManager;

    [Header ("Players")]
    [SerializeField] EnemyInitializer enemyInitializer;
    [SerializeField] PlayerInitializer playerInitializer;

    [SerializeField] List<OpponentData> datas;
    [SerializeField] PlayerData playerData;

    [SerializeField] PlayerView opponentView1;
    [SerializeField] OpponentView opponentView2;

    [SerializeField] bool isAiBattle = true;

    private void Start() {
        InitGame().Forget();
    }

    private async UniTask InitGame() {
        LocationData locationData = _locationManager.GetSceneLocation();
        await loadingManager.LoadResourcesForLocation(locationData);

        (Opponent first, Opponent second) opponents = CreateOpponets(isAiBattle);
        StartBattle(opponents.first, opponents.second);
    }

    private void StartBattle(Opponent opponent1, Opponent opponent2) {
        if (opponent1 != null && opponent2 != null) {
            boardGame.StartBattle(opponent1, opponent2);
            opponent1.DrawCards(5);
            opponent2.DrawCards(5);
        }
    }

    private (Opponent first, Opponent second) CreateOpponets(bool isAiBattle) {
        Opponent opponent1;

        OpponentData opponentData1 = datas[1];
        OpponentPresenter opponentPresenter2 = enemyInitializer.CreateOpponent(opponentData1, opponentView2);
        Opponent opponent2 = opponentPresenter2.Opponent;

        if (isAiBattle) {
            OpponentData opponentData2 = datas[0];
            OpponentPresenter opponentPresenter = enemyInitializer.CreateOpponent(opponentData2, opponentView1);
            opponent1 = opponentPresenter.Opponent;
        } else {
            OpponentPresenter opponentPresenter = playerInitializer.CreateOpponent(playerData, opponentView1);
            opponent1 = opponentPresenter.Opponent;
        }

        return (opponent1, opponent2);
    }
}