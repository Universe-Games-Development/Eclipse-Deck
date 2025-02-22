using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

public class CreatureSpawner : MonoBehaviour {
    
    [SerializeField] private CreatureController creaturePrefab;
    [SerializeField] private float height = 1f;

    // Например, через Zenject или через публичное свойство
    [Inject] GameEventBus eventBus;
    [Inject] private GridVisual boardVisual;
    [Inject] private MovementStrategyFactory movementStrategyFactory;

    public async UniTask<bool> SpawnCreature(CreatureCard creatureCard, Field targetField, Opponent summoner) {
        Creature creature = new Creature(creatureCard, movementStrategyFactory, eventBus);
        bool isSummoned = targetField.SummonCreature(creature, summoner);
        if (!isSummoned) {
            Debug.LogWarning("Fail to spawn in logic");
            return false;
        }
        FieldController fc = boardVisual.GetController(targetField);
        if (fc == null) {
            Debug.LogWarning("FieldController not found for target field.");
            return false;
        }

        Transform spawnOrigin = fc.GetSpawnOrigin();
        Vector3 spawnPosition = spawnOrigin.position + Vector3.up * height;
        CreatureController creatureController = Instantiate(creaturePrefab, spawnPosition, Quaternion.identity, spawnOrigin);
        creatureController.SetView(creatureCard.creatureCardData.viewPrefab);
        creatureController.Initialize(creatureCard);
        await UniTask.CompletedTask;
        return true;
    }
}

