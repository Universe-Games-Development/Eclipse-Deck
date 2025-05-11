using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public interface IMover {
    UniTask MoveAsync(Transform target, Vector3 destination, float duration);
}


public class ObjectMover : IMover {
    public virtual async UniTask MoveAsync(Transform target, Vector3 destination, float duration) {
        if (target == null) return;

        Vector3 start = target.position;
        float elapsed = 0f;

        while (elapsed < duration) {
            if (target == null) return;

            float t = elapsed / duration;
            target.position = Vector3.Lerp(start, destination, t);
            elapsed += Time.deltaTime;
            await UniTask.Yield();
        }

        // Гарантуємо точну установку фінальної позиції
        if (target != null)
            target.position = destination;
    }
}

public class DoTweenMover : IMover {
    public virtual async UniTask MoveAsync(Transform target, Vector3 destination, float duration) {
        if (target == null) return;

        // Створюємо UniTask completion source для очікування завершення твіна
        var completionSource = new UniTaskCompletionSource();

        await target.DOMove(destination, duration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => completionSource.TrySetResult());
    }
}
