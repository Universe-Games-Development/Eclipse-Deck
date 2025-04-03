using System;
using UnityEngine;
using Zenject;

public class EnemyPresenter : MonoBehaviour {
    [SerializeField] EnemyView view;

    [Inject] OpponentRegistrator opponentRegistrator;
    [Inject] DialogueSystem dialogueSystem;
    [Inject] GameEventBus eventBus;

    public Enemy Enemy { get; private set; }
    private Speaker speech;

    public void InitializeEnemy(Enemy enemy, Transform spawnPoint) {
        Enemy = enemy;

        // Устанавливаем позицию врага в точке спавна
        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;

        // Инициализируем диалоговую систему, если есть данные для речи
        if (enemy.Data != null && enemy.Data.speechData != null) {
            speech = new Speaker(enemy.Data.speechData, enemy, dialogueSystem, eventBus);
        }

        // Регистрируем врага и подписываемся на событие поражения
        enemy.OnDefeat += OnDefeatActions;
        opponentRegistrator.RegisterOpponent(enemy);

        // Инициализируем представление
        view.Initialize(enemy.Data);
    }

    private void OnDefeatActions(Opponent opponent) {
        // Публикуем событие о поражении врага
        eventBus.Raise(new EnemyDefeatedEvent(opponent));

        // Отписываемся от событий
        if (Enemy != null) {
            Enemy.OnDefeat -= OnDefeatActions;
        }

        // Можно добавить визуальные эффекты поражения
        // view.PlayDefeatAnimation();
    }

    private void OnDestroy() {
        // Очистка подписок при уничтожении объекта
        if (Enemy != null) {
            Enemy.OnDefeat -= OnDefeatActions;
        }
    }
}

