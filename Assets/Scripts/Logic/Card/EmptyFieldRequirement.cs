public class EmptyFieldRequirement : IRequirement<Field> {
    public bool IsMet(Field field, out string errorMessage) {
        var result = field?.OccupiedCreature == null;
        errorMessage = result ?
            string.Empty :
            "This field not empty";
        return result;
    }

    public string GetInstruction() => "Empty field";
}

public class OwnerFieldRequirement : IRequirement<Field> {
    private Opponent requestingPlayer;

    public OwnerFieldRequirement(Opponent requestingPlayer) {
        this.requestingPlayer = requestingPlayer;
    }

    public bool IsMet(Field field, out string errorMessage) {
        var result = field?.Owner == requestingPlayer;
        errorMessage = result ?
            string.Empty :
            "This field does not belong to you";
        return result;
    }

    public string GetInstruction() => "Field must belong to you";
}