using Cysharp.Threading.Tasks;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

[RequireComponent (typeof(CreatureAnimator))]
public class CreaturePresenter : MonoBehaviour {
    [Inject] private GridVisual gridVisual;
    [Inject] private CreatureBehaviour creatureBehaviour;
    [Inject] private GameEventBus eventBus;

    public Creature Model { get; private set; }
    
    [SerializeField] private CreatureAnimator dotweenAnimator;
    private CreatureView view;
    [SerializeField] private Transform viewParent;

    public void Initialize(CreatureCard creatureCard, Field targetField) {
        // creating model
        Model = new Creature(creatureCard, creatureBehaviour, eventBus);
        Model.OnSpawned += OnCreatureSpawned;
        Model.OnMoved += OnCreatureMoved;
        Model.OnInterruptedMove += OnCreatureMoveInterrupted;
        // Creating view
        view = Instantiate(Model.creatureCard.creatureCardData.viewPrefab, viewParent);
        Model.Spawn(targetField);
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
        FieldPresenter fc = gridVisual.GetController(targetField);
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
        if (Model == null) return;

        Model.OnMoved -= OnCreatureMoved;
        Model.OnSpawned -= OnCreatureSpawned;
    }
}
