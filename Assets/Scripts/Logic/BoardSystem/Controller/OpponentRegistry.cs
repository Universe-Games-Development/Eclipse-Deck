using System.Collections.Generic;
using UnityEngine;

public class OpponentRegistry : MonoBehaviour {
    [SerializeField] public BoardPlayerPresenter _boardPlayer;
    [SerializeField] public BoardPlayerPresenter _boardEnemy;
    private Dictionary<Opponent, BoardPlayerPresenter> _activePlayers = new();

    public void RegisterOpponent(Opponent character) {
        var opponentRepresenter = character is Player ? _boardPlayer : _boardEnemy;
        opponentRepresenter.BindPlayer(character);
        _activePlayers[character] = opponentRepresenter;
    }

    public Opponent GetOpponent(Opponent player) {
        foreach (var opponent in _activePlayers.Keys) {
            if (opponent != player) {
                return opponent;
            }
        }
        return null; // Якщо опонент не знайдено
    }
}

