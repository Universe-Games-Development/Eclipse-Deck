using System.Collections.Generic;
using UnityEngine;

public class OpponentRepresentator : MonoBehaviour {
    [SerializeField] private BoardPlayer _boardPlayer;
    [SerializeField] private BoardPlayer _boardEnemy;
    private Dictionary<CharacterPresenter, BoardPlayer> _activePlayers = new();

    public void RegisterOpponent(CharacterPresenter presenter) {
        var opponentRepresenter = presenter.Model is Player ? _boardPlayer : _boardEnemy;
        opponentRepresenter.BindPlayer(presenter);
        _activePlayers[presenter] = opponentRepresenter;
    }
}

