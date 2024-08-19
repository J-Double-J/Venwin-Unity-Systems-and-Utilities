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
    }

}
