using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;

public interface ITargetFiller {
    bool CanFillTargets(List<TargetInfo> targets, string ownerId);
    UniTask<TargetOperationResult> FillTargetsAsync(TargetOperationRequest request, CancellationToken cancellationToken = default);
    void RegisterSelector(string playerId, ITargetSelectionService selectionService);
    UniTask<TargetFillResult> TryFillTargetAsync(TargetInfo target, UnitModel requestSource, bool isMandatory, CancellationToken cancellationToken = default);
    void UnregisterSelector(string playerId);
}