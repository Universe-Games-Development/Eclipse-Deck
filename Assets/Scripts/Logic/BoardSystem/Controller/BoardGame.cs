using UnityEngine;
using Zenject;

public class BoardGame : MonoBehaviour {
    [SerializeField] private BoardSettingsData boardConfig;
    [SerializeField] private BoardSystem boardSystem;
    
    [SerializeField] private CreatureSpawner creatureSpawner;

    [SerializeField] public OpponentRepresentator PlayersRepresentation;

    [Inject] GameEventBus _eventBus;

    private void Start() {
        PrepareBattle();
    }

    public void PrepareBattle() {
        boardSystem.Initialize();
        //boardSystem.UpdateGrid(boardConfig);

        BeginBattle();
    }

    public void BeginBattle() {
        _eventBus.Raise(new BattleStartedEvent());
    }

    private void OnDrawGizmosSelected() {
        if (boardConfig == null) return;

        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);

        int totalRows = boardConfig.eastColumns + boardConfig.southRows;
        int totalColumns = boardConfig.northRows + boardConfig.westColumns;

        Vector2 cellSize = new Vector2(boardConfig.cellSize.width, boardConfig.cellSize.height);

        // Центруємо грід відносно позиції
        Vector3 origin = transform.position - new Vector3(
            (totalColumns * cellSize.x) / 2f - cellSize.x / 2f,
            0,
            (totalRows * cellSize.y) / 2f - cellSize.y / 2f
        );

        for (int row = 0; row < totalRows; row++) {
            for (int col = 0; col < totalColumns; col++) {
                Vector3 cellCenter = origin + new Vector3(col * cellSize.x, 0, row * cellSize.y);
                Gizmos.DrawWireCube(cellCenter, new Vector3(cellSize.x, 0.01f, cellSize.y));
            }
        }
    }

    public void TookSeat(OpponentPresenter opponentPresenter) {
        PlayersRepresentation.RegisterOpponent(opponentPresenter);
    }
}

