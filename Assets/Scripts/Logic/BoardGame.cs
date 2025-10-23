using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class BoardGame : MonoBehaviour
{
    [Inject] IOpponentRegistry opponentRegistry;
    [Inject] TurnManager turnManager;
    [SerializeField] BoardManager boardManager;

    public void StartBattle(Opponent player1, Opponent player2) {
        opponentRegistry.RegisterOpponent(player1);
        opponentRegistry.RegisterOpponent(player2);

        boardManager.CreateBoard(2, 2);
        boardManager.SpawnSummongZonesAtColums(2);

        boardManager.AssignRowTo(0, player1);
        boardManager.AssignRowTo(1, player2);

        turnManager.InitTurns(new List<Opponent>() { player1, player2 });

        Debug.Log("Battle Started!");
    }
}
