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
        await UniTask.WhenAll(
            gameBoardSeats.AssignEnemySeat(enemy.Enemy, enemy),
            gameBoardSeats.AssignPlayerSeat(player.Player, player)
        );
        BoardAssigner boardAssigner = new BoardAssigner(gameBoardSeats, boardPresenter);
        boardPresenter.Initialize(boardAssigner);
        boardPresenter.UpdateGrid(boardConfig);
    }

    public void BeginBattle() {
        throw new NotImplementedException();
    }

    public bool SpawnCreature(CreatureCard creatureCard, Field field, Opponent summoner) {
        throw new NotImplementedException();
    }
}

