using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using Zenject;

public class CreatureController : MonoBehaviour {
    [Inject] GridVisual gridVisual;
    [Inject] CraetureBehaviour craetureBehaviour;

    [Inject] GameEventBus eventBus;
    public Creature Creature { get; internal set; }
    
    [SerializeField] Transform viewParent;
    [SerializeField] private float spawnHeightOffset = 1f;
    public void Initialize(CreatureCard creatureCard, Field targetField) {
        Creature = new Creature(creatureCard, craetureBehaviour, eventBus);
        Creature.OnMoved += PerformMovement;
        Creature.OnSpawned += SpawnOnField;
        SetView(creatureCard.creatureCardData.viewPrefab);
        Creature.Spawn(targetField);
    }

    private async UniTask PerformMovement(Field targetField) {
        Transform creaturePoint = GetCreatureTransformPoint(targetField);
        transform.SetParent(creaturePoint);

        Vector3 creaturePointPosition = creaturePoint.position;
        await transform.DOMove(creaturePointPosition, 1).AsyncWaitForCompletion();
        transform.position = creaturePointPosition;
    }

    public async UniTask SpawnOnField(Field targetField) {
        Transform creaturePoint = GetCreatureTransformPoint(targetField);
        transform.SetParent(creaturePoint);

        Vector3 originPosition = creaturePoint.position;
        transform.position = originPosition + Vector3.up * spawnHeightOffset; // spawn above the field

        await transform.DOLocalMove(Vector3.zero, 1).AsyncWaitForCompletion();
    }
    public void SetView(CreatureView view) {
        CreatureView viewObject = Instantiate(view, viewParent);
    }

    private Vector3 GetCreatureFieldPosition(Field targetField) {
        Transform creaturePoint = GetCreatureTransformPoint(targetField);
        if (creaturePoint == null) {
            return Vector3.zero;
        }
        return creaturePoint.position;
    }

    private Transform GetCreatureTransformPoint(Field targetField) {
        FieldController fc = gridVisual.GetController(targetField);
        if (fc == null) {
            Debug.LogWarning("FieldController not found for target field.");
            return null;
        }

        return fc.GetCreaturePlace();
    }

    private void OnDestroy() {
        if (Creature != null && Creature.OnMoved != null) {
            Creature.OnMoved -= PerformMovement;
        }
    }
}
