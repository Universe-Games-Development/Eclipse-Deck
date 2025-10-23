using Cysharp.Threading.Tasks;
using System;
using System.Threading;

public class VisualSequenceManager : TaskQueueManager<VisualTask>, IVisualManager {
    protected override LogCategory LogCategory => LogCategory.Visualmanager;
    protected override string TaskTypeName => "visual task";

    protected override async UniTask<ExecutionResult> ProcessTaskAsync(VisualTask task, CancellationToken cancellationToken) {
        try {
            if (task == null) {
                return ExecutionResult.Failure("Task is null");
            }

            bool result = await task.ExecuteAsync();
            return result ? ExecutionResult.Success() : ExecutionResult.Failure("Failed execution");
        } catch (OperationCanceledException) {
            throw;
        }
    }
}
