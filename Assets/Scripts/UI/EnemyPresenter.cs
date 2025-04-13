using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

public class EnemyPresenter : BaseOpponentPresenter {
    public Enemy Enemy => (Enemy) OpponentModel;
    private Speaker speech;
    [SerializeField] private EnemyView view;

    [Inject] protected BattleRegistrator opponentRegistrator;
    [Inject] protected GameEventBus eventBus;
    [Inject] private DialogueSystem dialogueSystem;
    [SerializeField] private bool dialogueEnabled = false;

    public void InitializeEnemy(Enemy enemy) {
        base.Initialize(enemy);
        view.Initialize(enemy.Data);
        // Initialize dialogue system if speech data exists
        if (enemy.Data != null && enemy.Data.speechData != null) {
            speech = new Speaker(enemy.Data.speechData, enemy, dialogueSystem, eventBus);
        }
    }

    public async UniTask StartEnemyActivity() {
        await view.PlayAppearAnimation();

        if (dialogueEnabled && speech != null) {
            await speech.StartDialogue();
        }

        // Register in the battle system
        opponentRegistrator.RegisterEnemy(this);
    }
}