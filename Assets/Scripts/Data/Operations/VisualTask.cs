using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public abstract class VisualTask : IExecutableTask {
    public abstract UniTask<bool> Execute();
}

public class UniversalVisualTask : VisualTask {
    private readonly Func<UniTask<bool>> _taskFunction;
    private readonly string _description;

    public UniversalVisualTask(Func<UniTask<bool>> taskFunction, string description = "") {
        _taskFunction = taskFunction;
        _description = string.IsNullOrEmpty(description) ? taskFunction?.Method.Name : description;
    }

    public UniversalVisualTask(Func<UniTask> taskFunction, string description = "")
        : this(async () => {
            await (taskFunction?.Invoke() ?? UniTask.CompletedTask);
            return true;
        }, description) {
    }

    public UniversalVisualTask(UniTask task, string description = "")
        : this(async () => {
            await task;
            return true;
        }, description) {
    }

    public UniversalVisualTask(Action action, string description = "")
        : this(() => {
            action?.Invoke();
            return UniTask.CompletedTask;
        }, description) {
    }

    public override async UniTask<bool> Execute() {
        if (_taskFunction == null) {
            Debug.LogWarning("UniversalVisualTask has null task function");
            return false;
        }

        Debug.Log($"Executing visual task: {_description}");
        var result = await _taskFunction();
        Debug.Log($"Visual task completed: {_description} - Result: {result}");
        return result;
    }
}
