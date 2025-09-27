using UnityEngine;

public class SimpleBoardTests : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        DoTest();
    }

    private void DoTest() {
        Board board = SetupInitialBoard();

        Creature dragon = GenerateTestCreature("Dragon");
        OperationResult operationResult = board.RemoveColumn(0);
    }

    private Board SetupInitialBoard() {

        var config = new BoardConfiguration()

            .AddRow(2, 3, 2)

            .AddRow(1, 4, 1)

            .AddRow(3, 2, 3);

        return new Board(config);

    }

    private Creature GenerateTestCreature(string name = null) {
        var cardData = ScriptableObject.CreateInstance<CreatureCardData>();
        Creature creature = new Creature(new CreatureCard(cardData, new Health(1), new Attack(1)));
        if (name != null)
        creature.SetName(name);
        return creature;

    }
}
