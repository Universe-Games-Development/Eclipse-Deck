using Cysharp.Threading.Tasks;
using System;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

[RequireComponent (typeof(CreatureAnimator))]
public class CreaturePresenter : MonoBehaviour {
    private BoardPresenter _boardPresenter;
    [Inject] private CreatureBehaviour creatureBehaviour;
    [Inject] private GameEventBus eventBus;

    public Creature Creature { get; private set; }
    
    [SerializeField] private CreatureAnimator dotweenAnimator;
    private CreatureView view;
    [SerializeField] private Transform viewParent;

    public void Initialize(CreatureCard creatureCard, Field targetField, BoardPresenter boardPresenter) {
        // creating model
        _boardPresenter = boardPresenter;
        Creature = new Creature(creatureCard, creatureBehaviour, eventBus);
        Creature.OnSpawned += OnCreatureSpawned;
        Creature.OnMoved += OnCreatureMoved;
        Creature.OnInterruptedMove += OnCreatureMoveInterrupted;
        // Creating view
        view = Instantiate(Creature.creatureCard.creatureCardData.viewPrefab, viewParent);
        Creature.Spawn(targetField);
    }

    private async UniTask OnCreatureSpawned(Field targetField) {
        Transform targetPoint = GetCreatureTransformPoint(targetField);
        if (targetPoint != null) {
            await dotweenAnimator.SpawnOnField(targetPoint);
        }
    }

    private async UniTask OnCreatureMoved(Field targetField) {
        Transform targetPoint = GetCreatureTransformPoint(targetField);
        if (targetPoint != null) {
            await dotweenAnimator.MoveToField(targetPoint);
        }
    }

    private async UniTask OnCreatureMoveInterrupted(Field invalidField) {
        Transform targetPoint = GetCreatureTransformPoint(invalidField);
        if (targetPoint != null) {
            await dotweenAnimator.InterruptedMove(targetPoint);
        }
    }

    private Transform GetCreatureTransformPoint(Field targetField) {
        FieldPresenter fc = _boardPresenter.GetFieldPresenter(targetField);
        if (fc == null) {
            Debug.LogError($"FieldController not found for {targetField}");
            return null;
        }

        Transform point = fc.GetCreaturePlace();
        if (point == null) {
            Debug.LogError($"Creature place not set in {fc.name}");
        }
        return point;
    }

    private void OnDestroy() {
        if (Creature == null) return;

        Creature.OnMoved -= OnCreatureMoved;
        Creature.OnSpawned -= OnCreatureSpawned;
    }
}
