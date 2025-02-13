using Cysharp.Threading.Tasks;
using UnityEngine;

public class EmptyCommand : Command {
    public async override UniTask Execute() {
        Debug.Log("Empty command");
        await UniTask.CompletedTask;
    }

    public async override UniTask Undo() {
        Debug.Log("Empty undo command");
        await UniTask.CompletedTask;
    }
}