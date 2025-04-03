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

        // ������������� ������� ����� � ����� ������
        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;

        // �������������� ���������� �������, ���� ���� ������ ��� ����
        if (enemy.Data != null && enemy.Data.speechData != null) {
            speech = new Speaker(enemy.Data.speechData, enemy, dialogueSystem, eventBus);
        }

        // ������������ ����� � ������������� �� ������� ���������
        enemy.OnDefeat += OnDefeatActions;
        opponentRegistrator.RegisterOpponent(enemy);

        // �������������� �������������
        view.Initialize(enemy.Data);
    }

    private void OnDefeatActions(Opponent opponent) {
        // ��������� ������� � ��������� �����
        eventBus.Raise(new EnemyDefeatedEvent(opponent));

        // ������������ �� �������
        if (Enemy != null) {
            Enemy.OnDefeat -= OnDefeatActions;
        }

        // ����� �������� ���������� ������� ���������
        // view.PlayDefeatAnimation();
    }

    private void OnDestroy() {
        // ������� �������� ��� ����������� �������
        if (Enemy != null) {
            Enemy.OnDefeat -= OnDefeatActions;
        }
    }
}

