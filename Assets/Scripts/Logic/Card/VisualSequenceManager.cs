using Cysharp.Threading.Tasks;
using System;
using System.Threading;

public class VisualSequenceManager : BaseQueueManager<VisualTask>, IVisualManager {
    protected override LogCategory LogCategory => LogCategory.Visualmanager;
    protected override string TaskTypeName => "visual task";

    protected override async UniTask<OperationResult> ProcessTaskAsync(VisualTask task, CancellationToken cancellationToken) {
        try {
            if (task == null) {
                return OperationResult.Failure("Task is null");
            }

            logger.LogInfo($"Beginning task: {task}", LogCategory);
            bool result = await task.Execute();
            return result ? OperationResult.Success() : OperationResult.Failure("Failed execution");
        } catch (OperationCanceledException) {
            logger.LogInfo($"Task {task} was cancelled", LogCategory);
            throw;
        }
    }
}
