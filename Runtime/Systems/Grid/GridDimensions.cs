using System;
using UnityEngine;

namespace Venwin.Grid
{
    [Serializable]
    public struct GridDimensions
    {
        [SerializeField] private int x;
        [SerializeField] private int z;
        [SerializeField] private int y;

        public readonly int X => x;
        public readonly int Z => z;
        public readonly int Y => y;

        public GridDimensions(int x, int y, int z)
        {
            if (x > 0) { this.x = x; }
            else
            {
                Debug.LogWarning("Cannot have an 'x' grid dimension that is 0 or less. Defaulting to 1");
                this.x = 1;
            }

            if (y > 0) { this.y = y; }
            else
            {
                Debug.LogWarning("Cannot have an 'y' grid dimension that is 0 or less. Defaulting to 1");
                this.y = 1;
            }

            if (z > 0) { this.z = z; }
            else
            {
                Debug.LogWarning("Cannot have an 'z' grid dimension that is 0 or less. Defaulting to 1");
                this.z = 1;
            }
        }
    }
}
