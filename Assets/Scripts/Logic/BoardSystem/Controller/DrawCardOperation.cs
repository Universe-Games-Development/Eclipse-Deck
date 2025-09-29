using Cysharp.Threading.Tasks;
using System.Collections.Generic;

public class DrawCardOperation : GameOperation {
    private OpponentPresenter opponentPresetner;
    private int _drawAmount;
    private List<Card> drawnCards;
    public DrawCardOperation(OpponentPresenter boardPlayer, int drawAmount = 1) {
        opponentPresetner = boardPlayer;
        _drawAmount = drawAmount;
    }

    public override async UniTask<bool> Execute() {
        opponentPresetner.DrawCards(_drawAmount);
        await UniTask.DelayFrame(1);
        return true;
    }
}
