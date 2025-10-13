using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class GameInitializer : MonoBehaviour {
    [SerializeField] BoardGame boardGame;
    [SerializeField] PlayerInitializer playerInitializer;
    [Inject] IOpponentFactory opponentFactory;
    [Inject] IPresenterFactory presenterFactory;
    [SerializeField] List<OpponentData> datas;
    [Inject] IUnitSpawner<Opponent, OpponentView, EnemyPresenter> enemySpawner;

    private void Start() {
        if (TryCreateEnemy(out Opponent enemy1) && TryCreateEnemy(out Opponent enemy2)) {
            enemySpawner.SpawnUnit(enemy1);
            enemySpawner.SpawnUnit(enemy2);

            boardGame.StartBattle(enemy1, enemy2);
        }
    }

    private bool TryCreateEnemy(out Opponent opponent) {
        if (!datas.TryGetRandomElement(out OpponentData data)) {
            Debug.LogWarning("Failed to get data");
            opponent = null;
            return false;
        }
        opponent = opponentFactory.CreateOpponent(data);
        return true;
    }
}