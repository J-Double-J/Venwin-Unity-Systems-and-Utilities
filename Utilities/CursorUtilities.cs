using UnityEngine;

namespace Venwin.Utilities
{
    public struct ClickInformation
    {
        public bool DidHit;
        public Vector3 Point;

        public ClickInformation(bool didHit, Vector3 point)
        {
            DidHit = didHit;
            Point = point;
        }
    }

    public static class CursorUtilities
    {
        /// <summary>
        /// Gets the clicked on <see cref="GameObject"/> if one was hit.
        /// </summary>
        /// <returns>The clicked <see cref="GameObject"/> if applicable, else null.</returns>
        public static GameObject CursorClickOnGameObject()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
            {
                return hit.transform.gameObject;
            }

            return null;
        }

        /// <summary>
        /// Gets the clicked on <see cref="GameObject"/> if one was hit on the correct layer.
        /// </summary>
        /// <param name="layerMask">Layers to check for a hit.</param>
        /// <returns>The clicked <see cref="GameObject"/> if applicable, else null.</returns>
        public static GameObject CursorClickOnGameObject(LayerMask layerMask)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
            {
                return hit.transform.gameObject;
            }

            return null;
        }

        public static ClickInformation CursorToWorldPosition(LayerMask layerMask)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
            {
                return new ClickInformation(true, hit.point);
            }

            return new ClickInformation(false, Vector3.zero);
        }
    }
}
