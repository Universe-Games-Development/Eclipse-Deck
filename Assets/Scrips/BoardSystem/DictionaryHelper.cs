using System.Collections.Generic;

public static class DirectionHelper {
    public static readonly Dictionary<Direction, (int rowOffset, int colOffset)> DirectionOffsets = new Dictionary<Direction, (int, int)> {
        { Direction.North,     (-1,  0) },
        { Direction.South,     ( 1,  0) },
        { Direction.East,      ( 0,  1) },
        { Direction.West,      ( 0, -1) },
        { Direction.NorthEast, (-1,  1) },
        { Direction.NorthWest, (-1, -1) },
        { Direction.SouthEast, ( 1,  1) },
        { Direction.SouthWest, ( 1, -1) }
    };
}
