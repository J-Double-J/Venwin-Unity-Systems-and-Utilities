using System;
using UnityEngine;

namespace Venwin.Grid
{
    [Serializable]
    public struct GridDimensions
    {
        [SerializeField] private int x;
        [SerializeField] private int z;

        public int X => x;
        public int Z => z;

        public GridDimensions(int x, int z)
        {
            if (x > 0) { this.x = x; }
            else
            {
                Debug.LogWarning("Cannot have an 'x' grid dimension that is 0 or less. Defaulting to 1");
                this.x = 1;
            }

            if (z > 0) { this.z = z; }
            else
            {
                Debug.LogWarning("Cannot have an 'x' grid dimension that is 0 or less. Defaulting to 1");
                this.z = 1;
            }
        }
    }
}
