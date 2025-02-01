using System.Collections.Generic;

public interface IMoveStrategy {
    public List<Path> CalculatePath(Field initialField);
}
