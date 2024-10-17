using UnityEngine;

namespace Venwin.Utilities
{
    public static class LayerMaskUtilities
    {
        public static bool AreLayerMasksEqual(this LayerMask mask1, LayerMask mask2)
        {
            return (mask1.value & mask2.value) == mask1.value && (mask1.value & mask2.value) == mask2.value;
        }

    }
}
