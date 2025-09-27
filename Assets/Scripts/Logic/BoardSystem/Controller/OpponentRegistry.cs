using System.Collections.Generic;
using UnityEngine;

public class OpponentRegistry : MonoBehaviour {
    [SerializeField] public Opponent player1;
    [SerializeField] public Opponent player2;

    public void RegisterOpponent(Opponent newOpponent) {
        player1 = newOpponent;
    }

    public Opponent GetOpponent(Opponent player) {
        if (player == player1) {
            return player2;
        } else if (player == player2) {
            return player1;
        }

        return null; // Якщо опонент не знайдено
    }
}

