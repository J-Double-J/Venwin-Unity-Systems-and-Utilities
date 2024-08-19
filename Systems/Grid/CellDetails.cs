using System;
using System.Collections.Generic;
using UnityEngine;

namespace Venwin.Grid
{
    /// <summary>
    /// Contains all the details regarding a specific cell.
    /// </summary>
    /// <remarks>
    /// Do not attempt to add multiple details that are the same class type. Cell Details assumes differing types in order
    /// to provide speedy access.
    /// </remarks>
    [Serializable]
    public class CellDetails
    {
        private Dictionary<string, object> detailsLookup;
        [SerializeField] List<MonoBehaviour> monoBehaviourDetails;
        [SerializeField] List<ScriptableObject> scriptableObjectDetails;

        public CellDetails()
        {
            detailsLookup = new Dictionary<string, object>();
        }

        public void AddDetail(object detail)
        {
            try
            {
                detailsLookup.Add(detail.GetType().Name, detail);
            }
            catch
            {
                Debug.LogError($"There was a problem adding a detail to a cell. " +
                    $"Likely there already exists a detail of the same type as `{detail.GetType().Name}`");
            }
        }

        /// <summary>
        /// Tries to add a detail to the cell.
        /// </summary>
        /// <param name="detail">Detail attempted to be added.</param>
        /// <param name="suppressWarning">If a failure occurs a debug warning is logged.
        /// Setting this parameter to true signals that failure is not a loggable offense.</param>
        /// <returns>Returns whether a detail was successfully added to the cell or not.</returns>
        public bool TryAddDetail(object detail, bool suppressWarning = false)
        {
            bool success = false;

            if (detailsLookup.TryAdd(detail.GetType().Name, detail))
            {
                return success;
            }
            else
            {
                if(!suppressWarning)
                {
                    Debug.LogWarning($"There was a problem trying to add a detail to a cell. " +
                        $"Likely there already exists a detail of the same type as `{detail.GetType().Name}`");
                }

                return false;
            }
        }

        public bool TryGetDetail<T>(out T foundDetail) where T : class
        {
            foundDetail = null;
            string typeName = typeof(T).Name;

            if (detailsLookup.TryGetValue(typeName, out object detail))
            {
                try
                {
                    foundDetail = (T)detail;
                    return true;
                }
                catch (InvalidCastException)
                {
                    Debug.LogError($"Attempted to retrieve a cell detail using the generic type `{typeof(T).Name}` " +
                        $"but the object stored in the dictionary was not an object of that type.");

                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}
