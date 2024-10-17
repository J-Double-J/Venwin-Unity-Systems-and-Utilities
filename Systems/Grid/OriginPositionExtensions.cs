using System;
using UnityEngine;

namespace Venwin.Grid
{
    /// <summary>
    /// Enum that outlines where the origin of a grid item is considered to start. Defaults to the bottom left corner of the grid.
    /// </summary>
    public enum OriginPosition
    {
        BottomLeft = 0,
        TopLeft = 1,
        TopRight = 2,
        BottomRight = 3
    }

    public static class OriginPositionExtensions
    {
        /// <summary>
        /// Rotates an <see cref="OriginPosition"/> 90 degrees clockwise.
        /// </summary>
        /// <param name="originPosition">The starting <see cref="OriginPosition"/>.</param>
        /// <returns>The ending <see cref="OriginPosition"/>.</returns>
        /// <exception cref="NotImplementedException">Thrown if unexpected <see cref="OriginPosition"/>.</exception>
        public static OriginPosition RotateClockwise(this OriginPosition originPosition)
        {
            return originPosition switch
            {
                OriginPosition.BottomLeft => OriginPosition.TopLeft,
                OriginPosition.TopLeft => OriginPosition.TopRight,
                OriginPosition.TopRight => OriginPosition.BottomRight,
                OriginPosition.BottomRight => OriginPosition.BottomLeft,
                _ => throw new NotImplementedException($"Origin position could not be rotated due to unknown state: {originPosition}"),
            };
        }

        /// <summary>
        /// Rotates an <see cref="OriginPosition"/> 90 degrees counter clockwise.
        /// </summary>
        /// <param name="originPosition">The starting <see cref="OriginPosition"/>.</param>
        /// <returns>The ending <see cref="OriginPosition"/>.</returns>
        /// <exception cref="NotImplementedException">Thrown if unexpected <see cref="OriginPosition"/>.</exception>
        public static OriginPosition RotateCounterClockwise(this OriginPosition originPosition)
        {
            return originPosition switch
            {
                OriginPosition.BottomLeft => OriginPosition.BottomRight,
                OriginPosition.TopLeft => OriginPosition.BottomLeft,
                OriginPosition.TopRight => OriginPosition.TopLeft,
                OriginPosition.BottomRight => OriginPosition.TopRight,
                _ => throw new NotImplementedException($"Origin position could not be rotated due to unknown state: {originPosition}"),
            };
        }

        /// <summary>
        /// Gets the <see cref="Quaternion"/> rotation from the <see cref="OriginPosition"/>. Assumes 0 deg starts at <see cref="OriginPosition.BottomLeft"/>.
        /// </summary>
        /// <param name="originPosition">Origin Position to extrapolate a quaternion value from.</param>
        /// <returns>A <see cref="Quaternion"/> based on the <see cref="OriginPosition"/>.</returns>
        public static Quaternion GetQuaternionBasedOnOriginPosition(this OriginPosition originPosition)
        {
            return originPosition switch
            {
                OriginPosition.BottomLeft => Quaternion.identity,
                OriginPosition.TopLeft => Quaternion.Euler(0, 90f, 0),
                OriginPosition.TopRight => Quaternion.Euler(0, 180f, 0),
                OriginPosition.BottomRight => Quaternion.Euler(0, 270f, 0),
                _ => Quaternion.identity,
            };
        }
    }
}
