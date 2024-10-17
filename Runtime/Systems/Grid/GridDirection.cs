using UnityEngine;

namespace Venwin.Grid
{
    public enum GridDirection
    {
        North = 0,
        East = 1,
        South = 2,
        West = 3
    }

    public static class GridDirectionExtensions
    {
        public static Vector3Int GetVectorFromDirection(this GridDirection direction)
        {
            return direction switch
            {
                GridDirection.North => new Vector3Int(0, 0, 1),
                GridDirection.East => new Vector3Int(1, 0, 0),
                GridDirection.South => new Vector3Int(0, 0, -1),
                GridDirection.West => new Vector3Int(-1, 0, 0),
                _ => Vector3Int.zero,
            };
        }

        public static GridDirection GetNewDirectionBasedOnOrientation(this GridDirection direction, OriginPosition origin)
        {
            int modifiedDirection = (int)direction + (int)origin;

            return (GridDirection)(modifiedDirection % 4);
        }
    }
}
