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

    private void Start() {
        board = SetupInitialBoard(rows, colums);
        boardPresenter = presenterFactory.CreatePresenter<BoardPresenter>(board, boardView);
        boardPresenter.CreateBoard();
        PopulateCells();
    }

    private Board SetupInitialBoard(int rows, int colums) {
        return new Board(rows, colums);
    }

    private void PopulateCells() {
        List<Cell> cells = board.GetAllCells();
        List<Zone> zones = CreateZones(cells.Count);

        for (int i = 0; i < cells.Count; i++) {
            ZonePresenter zonePresenter = SpawnTestZone(zones[i]);
            boardPresenter.AssignArea(cells[i], zones[i]);
        }
    }

    private List<Zone> CreateZones(int zonesCount) {
        List<Zone> zones = new();
        for (int i = 0; i < zonesCount; i++) {
            int randomSize = Random.Range(minSize, maxSize);
            zones.Add(CreateZone(randomSize));
        }
        return zones;
    }

    private Zone CreateZone(int size = 1) {
        List<Opponent> opponents = opponentRegistry.GetOpponents();
        Opponent player = opponents.First();

        Zone zone = entityFactory.Create<Zone>(size);
        zone.ChangeOwner(player.Id);
        return zone;
    }

    private ZonePresenter SpawnTestZone(Zone zone) {
        ZonePresenter presenter = zoneSpawner.SpawnUnit(zone);
        presenter.View.transform.SetParent(transform);
        presenter.View.transform.position = Vector3.zero;
        return presenter;
    }
}
