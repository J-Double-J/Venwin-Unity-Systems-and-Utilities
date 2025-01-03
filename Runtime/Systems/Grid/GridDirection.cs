using System;
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
        /// <summary>
        /// Gets a Vector3Int that is equivalent to a <see cref="GridDirection"/>.
        /// </summary>
        /// <param name="direction">Direction to convert from.</param>
        /// <returns>A <see cref="Vector3Int"/> representation of a <see cref="GridDirection"/>.</returns>
        public static Vector3Int GetVectorFromDirection(this GridDirection direction)
        {
            return direction switch
            {
                GridDirection.North => Vector3Int.forward,
                GridDirection.East => Vector3Int.right,
                GridDirection.South => Vector3Int.back,
                GridDirection.West => Vector3Int.left,
                _ => Vector3Int.zero,
            };
        }

        /// <summary>
        /// Gets a direction based on a grid orientation.
        /// </summary>
        /// <param name="direction">Current direction</param>
        /// <param name="origin">Rotation around the cell.</param>
        /// <returns>A rotated grid direction</returns>
        public static GridDirection GetNewDirectionBasedOnOrientation(this GridDirection direction, OriginPosition origin)
        {
            int modifiedDirection = (int)direction + (int)origin;

            return (GridDirection)(modifiedDirection % 4);
        }

        /// <summary>
        /// Converts from a <see cref="Vector3Int"/> to a <see cref="GridDirection"/>
        /// </summary>
        /// <param name="vector">Vector to convert from</param>
        /// <returns>A valid <see cref="GridDirection"/> if a conversion can be made.</returns>
        /// <exception cref="ArgumentException">Thrown if vector can't be converted. </exception>
        public static GridDirection GetDirectionFromVector3Int(Vector3Int vector)
        {
            if (vector == Vector3.forward) return GridDirection.North;
            else if (vector == Vector3.right) return GridDirection.East;
            else if (vector == Vector3.back) return GridDirection.South;
            else if (vector == Vector3.left) return GridDirection.West;
            else { throw new ArgumentException("Vector must be normalized in a standard direction to be converted to a GridDirection"); }
        }
    }
}
