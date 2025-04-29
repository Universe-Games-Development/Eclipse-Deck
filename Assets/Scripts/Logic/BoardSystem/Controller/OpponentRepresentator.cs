using System.Collections.Generic;
using UnityEngine;

public class OpponentRepresentator : MonoBehaviour {
    [SerializeField] private BoardPlayer _boardPlayer;
    [SerializeField] private BoardPlayer _boardEnemy;
    private Dictionary<Opponent, BoardPlayer> _activePlayers = new Dictionary<Opponent, BoardPlayer>();

    public void RegisterOpponent(OpponentPresenter presenter) {
        var model = presenter.Model;
        var opponentRepresenter = model is Player ? _boardPlayer : _boardEnemy;
        opponentRepresenter.BindPlayer(model);
        _activePlayers[model] = opponentRepresenter;
    }
}

