using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;

public interface ITargetFiller {
    bool CanFillTargets(List<TypedTargetBase> targets);
    UniTask<TargetOperationResult> FillTargetsAsync(TargetOperationRequest request, CancellationToken cancellationToken = default);
    void RegisterSelector(string opponentID, ITargetSelectionService selectionService);
    void UnRegisterSelector(string opponent);
}