using System.Collections.Generic;
using UnityEngine;

public class OpponentRepresentator : MonoBehaviour {
    [SerializeField] public BoardPlayer _boardPlayer;
    [SerializeField] public BoardPlayer _boardEnemy;
    private Dictionary<CharacterPresenter, BoardPlayer> _activePlayers = new();

    public void RegisterOpponent(CharacterPresenter presenter) {
        var opponentRepresenter = presenter.Model is Player ? _boardPlayer : _boardEnemy;
        opponentRepresenter.BindPlayer(presenter);
        _activePlayers[presenter] = opponentRepresenter;
    }

    public BoardPlayer GetOpponent(BoardPlayer player) {
        foreach (var opponent in _activePlayers.Values) {
            if (opponent != player) {
                return opponent;
            }
        }
        return null; // Якщо опонент не знайдено
    }
}

