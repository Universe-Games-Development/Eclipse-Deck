using System.Collections.Generic;

public static class CompassUtil {
    public static readonly Dictionary<Direction, (int rowOffset, int colOffset)> DirectionOffsets = new() {
        { Direction.North,     (-1,  0) },
        { Direction.South,     ( 1,  0) },
        { Direction.East,      ( 0,  1) },
        { Direction.West,      ( 0, -1) },
        { Direction.NorthEast, (-1,  1) },
        { Direction.NorthWest, (-1, -1) },
        { Direction.SouthEast, ( 1,  1) },
        { Direction.SouthWest, ( 1, -1) }
    };

    public static Direction GetOppositeDirection(Direction direction) {
        return direction switch {
            Direction.North => Direction.South,
            Direction.South => Direction.North,
            Direction.East => Direction.West,
            Direction.West => Direction.East,
            Direction.NorthEast => Direction.SouthWest,
            Direction.NorthWest => Direction.SouthEast,
            Direction.SouthEast => Direction.NorthWest,
            Direction.SouthWest => Direction.NorthEast,
            _ => throw new System.ArgumentException("Invalid direction", nameof(direction)),
        };
    }

    internal static List<(int rowOffset, int colOffset)> GetOffsets() {
        return new List<(int rowOffset, int colOffset)>(DirectionOffsets.Values);
    }

    public static Direction GetDirectionFromOffset(int rowOffset, int colOffset) {
        foreach (var direction in DirectionOffsets) {
            if (direction.Value.rowOffset == rowOffset && direction.Value.colOffset == colOffset) {
                return direction.Key;
            }
        }
        throw new System.ArgumentException("Invalid offset values", nameof(rowOffset));
    }
}
