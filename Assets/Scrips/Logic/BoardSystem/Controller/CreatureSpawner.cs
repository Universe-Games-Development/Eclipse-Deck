using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

public class CreatureSpawner : MonoBehaviour {
    
    [SerializeField] private CreatureController creaturePrefab;
    [Inject] DiContainer container;

    public async UniTask<bool> SpawnCreature(CreatureCard creatureCard, Field targetField, Opponent summoner) {
        bool canSummon = targetField.CanSummonCreature(summoner);
        if (!canSummon) {
            Debug.LogWarning("Can't summon creature on this field");
            return false;
        }
        
        CreatureController creatureController = container.InstantiatePrefabForComponent<CreatureController>(creaturePrefab);
        creatureController.Initialize(creatureCard, targetField);
        await UniTask.CompletedTask;
        return true;
    }
}

