using UnityEngine;

[CreateAssetMenu(fileName = "DrawCard", menuName = "Operations/DrawCard")]
public class DrawCardOperationData : OperationData {
    public int drawAmount = 1;

    public override GameOperation CreateOperation(IOperationFactory factory, TargetRegistry targetRegistry) {
        Opponent opponent = targetRegistry.Get<Opponent>(TargetKeys.MainTarget);

        return factory.Create<DrawCardOperation>(this, opponent, drawAmount);
    }

    protected override void BuildDefaultRequirements() {
    }
}

public class DrawCardOperation : GameOperation {
    private Opponent _opponent;
    private int _drawAmount;

    public DrawCardOperation(Opponent opponent, int drawAmount = 1) {
        _opponent = opponent;
        _drawAmount = drawAmount;
    }

    public override bool Execute() {
        _opponent.DrawCards(_drawAmount);
        return true;
    }
}
