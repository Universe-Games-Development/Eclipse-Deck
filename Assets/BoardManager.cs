using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;
using Zenject;

public class BoardManager : MonoBehaviour
{
    [Inject] IPresenterFactory presenterFactory;
    [SerializeField] BoardConfiguration boardConfiguration;
    [SerializeField] BoardView boardView;
    [Inject] IUnitSpawner<Zone, ZoneView, ZonePresenter> zoneSpawner;
    [Inject] IOpponentRegistry opponentRegistry;
    [Inject] IEntityFactory entityFactory;

    [SerializeField] int minSize = 4;
    [SerializeField] int maxSize = 8;

    [SerializeField] int colums = 3;
    [SerializeField] int rows = 3;

    private Board board;
    private BoardPresenter boardPresenter;
    [SerializeField] bool doPopulate = false;
    [SerializeField] bool doRandomChanges = false;

    [SerializeField] int zoneTryes = 3;

    private void Start() {
        board = SetupInitialBoard(rows, colums);
        boardPresenter = presenterFactory.CreatePresenter<BoardPresenter>(board, boardView);

        if (doPopulate) {
            PopulateCells();
        }

        boardPresenter.CreateBoard();
        StartTestChanges();
    }

    public void StartTestChanges() {
        if (!doPopulate) return;

        if (doRandomChanges) {
            DoRandomColumnChanges();
        }
    }

    private void DoRandomColumnChanges() {
        for (int i = 0; i < zoneTryes; i++) {
            bool doRemoveColumn = Random.Range(0, 3) > 1;
            if (doRemoveColumn) {
                int columns = board.GetCurrentColumnsCount() - 1;
                //Debug.Log($"REMOVE {columns}");
                board.RemoveColumn(columns);
            } else {
                board.AddColumn();
                //Debug.Log("ADD");
            }
        }
    }

    private Board SetupInitialBoard(int rows, int colums) {
        return new Board(rows, colums);
    }

    private void PopulateCells() {
        List<Cell> cells = board.GetAllCells();

        List<Zone> zones = CreatePlayerZones(cells.Count);

        for (int i = 0; i < cells.Count; i++) {
            ZonePresenter zonePresenter = SpawnTestZone(zones[i]);
        }
        boardPresenter.AssignAreas(cells, zones);
    }

    private List<Zone> CreatePlayerZones(int zonesCount) {
        List<Opponent> opponents = opponentRegistry.GetOpponents();
        Opponent player = opponents.First();
        
        List<Zone> zones = new();
        for (int i = 0; i < zonesCount; i++) {
            int randomSize = Random.Range(minSize, maxSize);
            Zone zone = entityFactory.Create<Zone>(randomSize);

            zone.ChangeOwner(player.Id);
            zones.Add(zone);
        }
        return zones;
    }

    private ZonePresenter SpawnTestZone(Zone zone) {
        ZonePresenter presenter = zoneSpawner.SpawnUnit(zone);
        presenter.View.transform.SetParent(transform);
        presenter.View.transform.position = Vector3.zero;
        return presenter;
    }
}
