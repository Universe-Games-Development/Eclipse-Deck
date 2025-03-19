using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Zenject;

public class EnemyView : MonoBehaviour {
    [SerializeField] private Animator animator;
    private EnemyData enemyData;
    public void Initialize(EnemyData enemyData) {
        this.enemyData = enemyData;
        // Soon we will use this data to display the enemy's name and other information
    }
}

public class EnemyPresenter : MonoBehaviour {
    [SerializeField] EnemyView view;
    [Inject] public Enemy enemy; // model

    [Inject] DialogueSystem dialogueSystem;
    [Inject] GameEventBus eventBus;
    [SerializeField] EnemyData data;
    [Inject] TurnManager turnManager;

    private Speaker speech;
    private void Awake() {
        if (data != null && data.speechData != null) {
            speech = new Speaker(data.speechData, dialogueSystem, turnManager, eventBus);
        }
    }

    private void OnDestroy() {
        speech?.Dispose();
    }
}
