using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;

public interface ITargetFiller {
    bool CanFillTargets(List<TypedTargetBase> targets);
    ITargetSelector CreateSelector(TypedTargetBase typeTarget, Opponent initiator);
    UniTask<TargetOperationResult> FillTargetsAsync(TargetOperationRequest request, CancellationToken cancellationToken = default);
}