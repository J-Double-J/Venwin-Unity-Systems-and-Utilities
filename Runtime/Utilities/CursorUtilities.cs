using UnityEngine;

#nullable enable

namespace Venwin.Utilities
{
    public struct CursorInformation
    {
        public bool DidHit;
        public Vector3 Point;

        public CursorInformation(bool didHit, Vector3 point)
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
        public static GameObject? CursorClickOnGameObject()
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
        public static GameObject? CursorClickOnGameObject(LayerMask layerMask)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
            {
                return hit.transform.gameObject;
            }

            return null;
        }

        public static CursorInformation CursorToWorldPosition(LayerMask layerMask)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
            {
                return new CursorInformation(true, hit.point);
            }

            return new CursorInformation(false, Vector3.zero);
        }

        /// <summary>
        /// Attempts to get a <see cref="GameObject"/> of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type of <see cref="GameObject"/> to look for on layer under mouse cursor.</typeparam>
        /// <param name="layerMask"><see cref="LayerMask"/> to search for type of <see cref="GameObject"/>.</param>
        /// <returns>A tuple that contains hit information if layer was hit, as well as a nullable object of type <typeparamref name="T"/>.</returns>
        public static (CursorInformation, T?) GameComponentAtCursorPosition<T>(LayerMask layerMask)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
            {
                return (new CursorInformation(true, hit.point), hit.transform.gameObject.GetComponent<T>());
            }

            return (new CursorInformation(false, Vector3.zero), default(T));
        }
    }
}
