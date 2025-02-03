using System.Collections.Generic;
using System.Linq;

public static class CompassUtil {
    public static readonly Dictionary<Direction, (int rowOffset, int colOffset)> DirectionOffsets = new() {
        { Direction.North,     (1,  0) },
        { Direction.South,     (-1,  0) },
        { Direction.East,      ( 0,  1) },
        { Direction.West,      ( 0, -1) },
        { Direction.NorthEast, ( 1,  1) },
        { Direction.NorthWest, ( 1, -1) },
        { Direction.SouthEast, (-1,  1) },
        { Direction.SouthWest, (-1, -1) }
    };

    private static readonly Dictionary<(int rowOffset, int colOffset), Direction> OffsetToDirection =
    DirectionOffsets.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

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

    private static readonly Dictionary<Direction, Direction[]> GlobalDirectionMapping = new() {
        { Direction.North, new[] { Direction.North, Direction.NorthEast, Direction.NorthWest } },
        { Direction.South, new[] { Direction.South, Direction.SouthEast, Direction.SouthWest } },
        { Direction.East, new[] { Direction.East, Direction.NorthEast, Direction.SouthEast } },
        { Direction.West, new[] { Direction.West, Direction.NorthWest, Direction.SouthWest } }
    };

    public static bool BelongsToGlobalDirection(Direction direction, Direction globalDirection) {
        return GlobalDirectionMapping.TryGetValue(globalDirection, out var validDirections) && validDirections.Contains(direction);
    }

    public static List<(int rowOffset, int colOffset)> GetOffsets() {
        return new List<(int rowOffset, int colOffset)>(DirectionOffsets.Values);
    }

    public static Direction GetDirectionFromOffset(int meridian, int zonal) {
        if (OffsetToDirection.TryGetValue((meridian, zonal), out var direction)) {
            return direction;
        }
        throw new System.ArgumentException("Invalid offset values", nameof(meridian));
    }

    internal static bool BelongsToGlobalDirection(object gridDirection, Direction globalDirection) {
        throw new System.NotImplementedException();
    }
}
