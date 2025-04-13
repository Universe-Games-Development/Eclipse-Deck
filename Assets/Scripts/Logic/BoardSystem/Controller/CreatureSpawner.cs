using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

public class CreatureSpawner : MonoBehaviour {
    
    [SerializeField] private CreaturePresenter creaturePrefab;
    [SerializeField] private BoardPresenter boardPresenter;
    [Inject] DiContainer container;

    public async UniTask<bool> SpawnCreature(CreatureCard creatureCard, Field targetField) {
        bool canSummon = targetField.CanSummonCreature();
        if (!canSummon) {
            Debug.LogWarning("Can't summon creature on this field");
            return false;
        }
        
        CreaturePresenter creatureController = container.InstantiatePrefabForComponent<CreaturePresenter>(creaturePrefab);
        creatureController.Initialize(creatureCard, targetField, boardPresenter);
        await UniTask.CompletedTask;
        return true;
    }
}

