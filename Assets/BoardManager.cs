using Cysharp.Threading.Tasks;
using System.Collections.Generic;
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
        //StartInitialTest();
    }

    public void CreateBoard(int rows, int colums) {
        board = SetupInitialBoard(rows, colums);
        boardPresenter = presenterFactory.CreatePresenter<BoardPresenter>(board, boardView);
    }

    public void AssignRowTo(int rowIndex, Opponent player) {
        Row row = board.GetRow(rowIndex);
        foreach (var cell in row.Cells) {
            if (cell.AssignedUnit != null)
            cell.AssignedUnit.ChangeOwner(player.OwnerId);
        }
    }


    public void SpawnSummongZones(int columns) {
        for (int i = 0; i < columns; i++) {
            List<Cell> cells = board.GetColumn(i);

            for (int cellIndex = 0; cellIndex < cells.Count; cellIndex++) {
                Zone zone = CreateZone();
                cells[cellIndex].AssignUnit(zone);
            }
        }
    }


    private void StartInitialTest() {
        CreateBoard(rows,colums);

        if (doPopulate) {
            PopulateCells();
        }

        StartTestChanges();
    }


    private Board SetupInitialBoard(int rows, int colums) {
        return new Board(rows, colums);
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


    private void PopulateCells() {
        List<Cell> cells = board.GetAllCells();

        List<Zone> zones = CreateZones(cells.Count);
        foreach (var zone in zones) {
            ZonePresenter zonePresenter = SpawnTestZone(zone);
        }

        if (zones.Count > 0)
            boardPresenter.AssignAreas(cells, zones);

        AssignOwners(zones);
    }

    private bool AssignOwners(List<Zone> zones) {
        List<Opponent> opponents = opponentRegistry.GetOpponents();
        if (opponents == null || opponents.Count == 0) {
            Debug.LogWarning("Failed to get opponents");
            return false;
        }
        Opponent player = opponents.First();

        for (int i = 0; i < zones.Count; i++) {
            zones[i].ChangeOwner(player.OwnerId);
        }

        return true;
    }

    private List<Zone> CreateZones(int zonesCount) {
        List<Zone> zones = new();
        for (int i = 0; i < zonesCount; i++) {
            Zone zone = CreateZone();
            zones.Add(zone);
        }
        return zones;
    }

    private Zone CreateZone() {
        int randomSize = Random.Range(minSize, maxSize);
        return entityFactory.Create<Zone>(randomSize);
    }

    private ZonePresenter SpawnTestZone(Zone zone) {
        ZonePresenter presenter = zoneSpawner.SpawnUnit(zone);
        presenter.View.transform.SetParent(transform);
        presenter.View.transform.position = Vector3.zero;
        return presenter;
    }

}
