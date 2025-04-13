using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using Zenject;

public class BoardGame : MonoBehaviour {
    [SerializeField] private BoardSettingsData boardConfig;
    [SerializeField] private BoardPresenter boardPresenter;

    [SerializeField] private BoardSeatSystem gameBoardSeats;
    
    [SerializeField] private CreatureSpawner creatureSpawner;

    [Inject] BattleRegistrator opponentRegistrator;
    private void Awake() {
        if (opponentRegistrator.IsMatchReady) {
            PrepareBattle(
                opponentRegistrator.GetPlayerPresenter(), 
                opponentRegistrator.GetEnemyPresenter()
                );
        } else {
            opponentRegistrator.OnMatchSetup += PrepareBattle;
        }
    }

    private void PrepareBattle(PlayerPresenter player, EnemyPresenter enemy) {
        Initialize(player, enemy).Forget();
    }

    public async UniTask Initialize(PlayerPresenter player, EnemyPresenter enemy) {
        if (player == null) {
            Debug.LogError("PlayerPresenter is null");
        }
        if (enemy == null) {
            Debug.LogError("EnemyPresenter is null");
        }
        await UniTask.WhenAll(
            gameBoardSeats.AssignOpponentSeat(enemy),
            gameBoardSeats.AssignOpponentSeat(player)
        );
        BoardAssigner boardAssigner = new BoardAssigner(gameBoardSeats, boardPresenter);
        boardPresenter.Initialize(boardAssigner);
        boardPresenter.UpdateGrid(boardConfig);
        gameBoardSeats.InitializePlayersCards();
    }

    public void BeginBattle() {
        throw new NotImplementedException();
    }

    public bool SpawnCreature(CreatureCard creatureCard, Field field, Opponent summoner) {
        throw new NotImplementedException();
    }
}

