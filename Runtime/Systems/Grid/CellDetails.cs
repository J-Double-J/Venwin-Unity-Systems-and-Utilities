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
        [SerializeField] List<MonoBehaviour> monoBehaviourDetails = new();
        [SerializeField] List<ScriptableObject> scriptableObjectDetails = new();

        // This dictionary is used to look up specific types of details.
        private Dictionary<string, object> detailsLookup = new();

        private bool gridDetailsInitialized = false;
        private List<IGridDetail> gridDetails = new();
        
        private Dictionary<string, object> DetailsLookup
        {
            get
            {
                if(!gridDetailsInitialized)
                {
                    InitializeGridDetailsList();
                }

                return detailsLookup;
            }
            set
            {
                gridDetailsInitialized = true;
                detailsLookup = value;
            }
        }

        // This list is used for iterating through all the details that may have common triggers like Update(), or being placed on a Grid()
        public List<IGridDetail> GridDetails
        {
            get
            {
                if(!gridDetailsInitialized)
                {
                    InitializeGridDetailsList();
                }

                return gridDetails;
            }
            private set
            {
                gridDetails = value;
                gridDetailsInitialized = true;
            }
        }

        public void InitializeGridDetailsList()
        {
            foreach (var monoBehaviour in monoBehaviourDetails)
            {
                if (monoBehaviour is IGridDetail iDetail)
                {
                    gridDetails.Add(iDetail);
                }
                else
                {
                    Debug.LogWarning($"A monobehavior does not implement the IDetail interface, it will not be found in IGridDetail lookups. " +
                        $"Type: {monoBehaviour.GetType().Name}");
                }

                AddDetail(monoBehaviour);
            }

            foreach (var scriptableObject in scriptableObjectDetails)
            {
                if (scriptableObject is IGridDetail iDetail)
                {
                    gridDetails.Add(iDetail);
                }
                else
                {
                    Debug.LogWarning($"A scriptable object does not implement the IDetail interface, it will not be found in IGridDetail lookups. " +
                        $"Type: {scriptableObject.GetType().Name}");
                }

                AddDetail(scriptableObject);
            }

            gridDetailsInitialized = true;
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

            if (DetailsLookup.TryGetValue(typeName, out object detail))
            {
                try
                {
                    foundDetail = (T)detail;
                    return true;
                }
                catch (InvalidCastException) // This shouldn't happen
                {
                    Debug.LogError($"Attempted to retrieve a cell detail using the generic type `{typeof(T).Name}` " +
                        $"but the object stored in the dictionary was not an object of that type.");

                    return false;
                }
                catch (Exception e)
                {
                    Debug.LogError($"Unknown error occurred when trying to get detail: {e.GetType().Name} - {e.Message}");
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public void AssignCellToMonobehaviorDetails(GridCell gridCell)
        {
            foreach (var gridDetail in monoBehaviourDetails)
            {
                if (gridDetail is IGridDetailMonobehavior monoDetail)
                {
                    monoDetail.AssignedGridCell = gridCell;
                }
            }
        }

        public void UnassignCellOnMonobehaviorDetails()
        {
            foreach (var gridDetail in monoBehaviourDetails)
            {
                if (gridDetail is IGridDetailMonobehavior monoDetail)
                {
                    monoDetail.AssignedGridCell = null;
                }
            }
        }

        public void TriggerDetailsOnPlace(GridObject gridObject)
        {
            foreach(var gridDetail in GridDetails)
            {
                gridDetail.OnOwningGridObjectPlaced(gridObject);
            }
        }

        public void TriggerDetailsUpdates()
        {
            foreach (var gridDetail in GridDetails)
            {
                gridDetail.OnOwningGridObjectUpdate();
            }
        }

        public void TriggerDetailsOnGridRemoval(GridObject gridObject)
        {
            foreach (var gridDetail in GridDetails)
            {
                gridDetail.OnOwningGridObjectRemoved(gridObject);
            }
        }
    }
}
