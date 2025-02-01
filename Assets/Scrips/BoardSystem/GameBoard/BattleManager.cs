using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using Zenject;

public class BattleManager {
    public Action OnBattleStarted;
    public Action OnBattleFinished;

    [Inject] private OpponentRegistrator Registrator;
    [Inject] private BoardUpdater _boardUpdater;

    [Inject]
    private void Construct(OpponentRegistrator registrator) {
        Registrator = registrator;
        Registrator.OnOpponentsRegistered += HandleBattleStart;
    }

    public void HandleBattleStart(List<Opponent> opponents) {
        StartBattle().Forget();
    }

    public async UniTask StartBattle() {
        await _boardUpdater.SpawnBoard();
        OnBattleStarted?.Invoke();
    }
}