using System.Collections.Generic;
using Zenject;

public class GameInitializer : IInitializable {
    private readonly TurnManager _turnManager;
    private readonly BattleManager _battleManager;
    private readonly PlayManagerRegistrator _playManagerRegistrator;
    private readonly OpponentRegistrator _opponentRegistrator;

    public GameInitializer(
        BattleManager battleManager,
        TurnManager turnManager,
        PlayManagerRegistrator playManagerRegistrator,
        OpponentRegistrator opponentRegistrator
    ) {
        _turnManager = turnManager;
        _battleManager = battleManager;
        _playManagerRegistrator = playManagerRegistrator;
        _opponentRegistrator = opponentRegistrator;
    }

    public void Initialize() {
        _opponentRegistrator.OnOpponentsRegistered += HandleOpponentsRegistered;
        _opponentRegistrator.OnOpponentUnregistered += HandleOpponentsUnRegistered;
    }

    private void HandleOpponentsRegistered(List<Opponent> opponents) {
        _battleManager.StartBattle(opponents);
        _turnManager.InitTurns(opponents);
        _playManagerRegistrator.EnablePlayCardServices(opponents);
    }

    private void HandleOpponentsUnRegistered(Opponent loser) {
        _playManagerRegistrator.StopPlaying(loser);
        _battleManager.EndBattle(loser);
        _turnManager.ResetTurnManager();
    }

    public void Dispose() {
        _opponentRegistrator.OnOpponentsRegistered -= HandleOpponentsRegistered;
        _opponentRegistrator.OnOpponentUnregistered -= HandleOpponentsUnRegistered;
    }
}