using System.Collections.Generic;

public interface IMoveStrategy {
    public abstract List<Path> CalculatePath(Field currentField);
}