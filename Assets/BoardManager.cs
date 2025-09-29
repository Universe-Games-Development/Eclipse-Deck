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

    [SerializeField] int testZoneSize = 4;

    private void Start() {
        ZonePresenter zonePresenter = SpawnTestZone();

        Board board = SetupInitialBoard();
        BoardPresenter boardPresenter = presenterFactory.CreatePresenter<BoardPresenter>(board, boardView);
        //boardPresenter.CreateBoard();

        boardPresenter.AssignArea(0, 0, zonePresenter.Zone);
    }

    private Board SetupInitialBoard() {

        var config = new BoardConfiguration()

            .AddRow(2, 3, 2)

            .AddRow(1, 4, 1)

            .AddRow(3, 2, 3);

        return new Board(config);
    }

    private ZonePresenter SpawnTestZone() {
        List<Opponent> opponents = opponentRegistry.GetOpponents();
        Opponent player = opponents.First();

        Zone zone = entityFactory.Create<Zone>(testZoneSize);
        zone.ChangeOwner(player.Id);
        ZonePresenter presenter = zoneSpawner.SpawnUnit(zone);
        presenter.View.transform.SetParent(transform);
        presenter.View.transform.position = Vector3.zero;
        return presenter;
    }
}
