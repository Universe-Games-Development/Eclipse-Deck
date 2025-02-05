using Cysharp.Threading.Tasks;
using UnityEngine;

public class EmptyCommand : ICommand {
    public async UniTask Execute() {
        Debug.Log("Empty command");
        await UniTask.CompletedTask;
    }

    public async UniTask Undo() {
        Debug.Log("Empty undo command");
        await UniTask.CompletedTask;
    }
}