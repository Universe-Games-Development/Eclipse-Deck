internal class BattleEndEventData {
    private Opponent testLooser;
    private Opponent testWinner;

    public BattleEndEventData(Opponent testWinner, Opponent testLooser) {
        this.testWinner = testWinner;
        this.testLooser = testLooser;
    }
}