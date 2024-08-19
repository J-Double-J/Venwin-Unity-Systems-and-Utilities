using UnityEngine;

namespace Venwin.Utilities
{
    public static class TransformUtilities
    {
        /// <summary>
        /// Has the child ignore the parent's scaling that is being applied to it.
        /// </summary>
        /// <remarks>
        /// <b>WARNING</b>: This does not keep the child forever at this scale. Any further scaling that applies to the child's parent
        /// will scale the child accordingly. This is best used when a child has a scaled parent that won't rescale again.
        /// </remarks>
        /// <param name="child">Child transform to have it ignore its parent's scale.</param>
        public static void IgnoreParentScale(Transform child)
        {
            child.localScale = new Vector3(child.localScale.x / child.parent.localScale.x,
                child.localScale.y / child.parent.localScale.y,
                child.localScale.z / child.parent.localScale.z);
        }
    }
}
