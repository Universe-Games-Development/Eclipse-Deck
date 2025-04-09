using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

public class CreatureSpawner : MonoBehaviour {
    
    [SerializeField] private CreaturePresenter creaturePrefab;
    [SerializeField] private BoardPresenter boardPresenter;
    [Inject] DiContainer container;

    public async UniTask<bool> SpawnCreature(CreatureCard creatureCard, Field targetField, Opponent summoner) {
        bool canSummon = targetField.CanSummonCreature(summoner);
        if (!canSummon) {
            Debug.LogWarning("Can't summon creature on this field");
            return false;
        }
        
        CreaturePresenter creatureController = container.InstantiatePrefabForComponent<CreaturePresenter>(creaturePrefab);
        creatureController.Initialize(boardPresenter, creatureCard, targetField);
        await UniTask.CompletedTask;
        return true;
    }
}

