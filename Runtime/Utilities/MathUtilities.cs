using System;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using UnityEngine;

namespace Venwin.Utilities
{
    public static class MathUtilities
    {
        /// <summary>
        /// Takes a starting value and heads towards the mean value. If it would overshoot, this method instead just stops at the mean value.
        /// </summary>
        /// <param name="startingValue">Current value to mean revert.</param>
        /// <param name="mean">The value that the <paramref name="startingValue"/> is heading towards.</param>
        /// <param name="stepCount">The value determing how fast <paramref name="startingValue"/> should head towards <paramref name="mean"/>.</param>
        /// <returns>The value that is closer to the mean.</returns>
        public static int MeanReversion(int startingValue, int mean = 0, int stepCount = 1)
        {
            int newValue = startingValue;

            if (startingValue == mean)
            {
                return mean;
            }

            if (startingValue < mean)
            {
                newValue = startingValue + stepCount;
                return newValue > mean ? mean : newValue;
            }

            if (startingValue > mean)
            {
                newValue = startingValue - stepCount;
                return newValue < mean ? mean : newValue;
            }

            // Shouldn't be executed.
            return startingValue;
        }

        /// <summary>
        /// Converts a <see cref="Vector3"/> into a <see cref="Vector3Int"/> while respecting thresholds to avoid flooring past.
        /// </summary>
        /// <remarks>
        /// Typically a value like (2.3, -5.18e-18, 1.2) when converted to <see cref="Vector3Int"/> will be converted to (2, -1, 1) when it should probably<br/>
        /// be more appropriately (2, 0, 1) due to floating point precision. This method fixes that.
        /// </remarks>
        /// <param name="vector"><see cref="Vector3"/> to convert into <see cref="Vector3Int"/>.</param>
        /// <param name="floatingPointThreshold">Threshold for the floating point precision. If a value is past this threshold in relation to a rounded value, its floored.</param>
        /// <returns><see cref="Vector3Int"/> that compensates for floating point errors.</returns>
        public static Vector3Int VectorFloatToInt_WithErrorThreshold(this Vector3 vector, double floatingPointThreshold = 1e-10)
        {
            int x = FloatToInt_WithErrorThreshold(vector.x);
            int y = FloatToInt_WithErrorThreshold(vector.y);
            int z = FloatToInt_WithErrorThreshold(vector.z);

            return new Vector3Int(x, y, z);
        }

        /// <summary>
        /// Converts a float to an integer while keeping in mind the inaccuracies of floating point math.
        /// </summary>
        /// <example>
        /// 1.99999999999 -> 2 <br/>
        /// 1.98 -> 1
        /// </example>
        /// <param name="val"></param>
        /// <param name="floatingPointThreshold"></param>
        /// <returns></returns>
        public static int FloatToInt_WithErrorThreshold(this float val, double floatingPointThreshold = 1e-10)
        {
            return Math.Abs(val - Math.Round(val)) < floatingPointThreshold ? (int)Math.Round(val) : Mathf.FloorToInt(val);
        }
    }

}
