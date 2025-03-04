using Cysharp.Threading.Tasks;
using System.ComponentModel;
using UnityEngine;

internal class DrawCardCommand : Command {
    private Opponent opponent;
    private int drawAmount;

    public DrawCardCommand(Opponent opponent, int amount) {
        this.opponent = opponent;
        drawAmount = amount;
    }

    public async override UniTask Execute() {
        while (drawAmount > 0) {
            Card card = opponent.deck.DrawCard();
            if (card == null) {
                Debug.Log("Player doesn`t have more cards he need to take damage (TO DO Soon)");
                await UniTask.CompletedTask;
            }

            bool result = opponent.hand.AddCard(card);
            if (!result) {
                Debug.Log("Player doesn`t have more space for card it will discarded");
                opponent.discardDeck.AddCard(card);
            }

            drawAmount--;
        }
        
        await UniTask.CompletedTask;
    }

    public override UniTask Undo() {
        throw new System.NotImplementedException();
    }
}