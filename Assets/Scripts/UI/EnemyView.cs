using Cysharp.Threading.Tasks;
using UnityEngine;

public class EnemyView : MonoBehaviour {
    [SerializeField] private Animator animator;
    [SerializeField] private SplineMover splineMover; // Ссылка на компонент SplineMover

    private OpponentData enemyData;
    private GameObject viewModel;

    public void Initialize(OpponentData enemyData) {
        this.enemyData = enemyData;
        if (viewModel != null) {
            Destroy(viewModel);
        }
        viewModel = Instantiate(enemyData.ViewModel, gameObject.transform);
    }

    public async UniTask PlayAppearAnimation() {
        // Запускаем анимацию появления, если есть
        if (animator != null) {
            animator.SetTrigger("Appear");
        }

        if (splineMover != null) {
            await splineMover.MoveAlongSpline(transform);
        } else {
            // Альтернативное поведение, если SplineMover не назначен
            await UniTask.CompletedTask;
        }
    }

    public async UniTask MoveToPositionAsync(Transform target, float duration = 1f) {
        Vector3 start = transform.position;
        Quaternion startRot = transform.rotation;

        Vector3 end = target.position;
        Quaternion endRot = target.rotation;

        float elapsed = 0f;

        while (elapsed < duration) {
            float t = elapsed / duration;
            transform.position = Vector3.Lerp(start, end, t);
            transform.rotation = Quaternion.Slerp(startRot, endRot, t);
            elapsed += Time.deltaTime;
            await UniTask.Yield();
        }

        // Гарантуємо точну установку фінальної позиції
        transform.position = end;
        transform.rotation = endRot;
    }
}