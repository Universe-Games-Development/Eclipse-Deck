using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

public class EnemyView : OpponentView {
    [SerializeField] private Animator animator;
    [SerializeField] private SplineMover splineMover; // Ссылка на компонент SplineMover
    [Inject] AnimationsDebugSettings animationDebugSettings;
    [Inject] RoomSystem roomPresenter;
    public async UniTask PlayAppearAnimation() {
        // Запускаем анимацию появления, если есть
        if (animator != null) {
            animator.SetTrigger("Appear");
        }

        if (splineMover != null) {
            await splineMover.MoveAlongSpline(transform, roomPresenter.GetEntrySplineForEnemy(), animationDebugSettings.SkipAllAnimations);
        } else {
            // Альтернативное поведение, если SplineMover не назначен
            await UniTask.CompletedTask;
        }
    }
}