using System;
using System.Collections.Generic;
using UnityEngine;

#nullable enable

namespace Venwin.Grid
{
    /// <summary>
    /// Holds details for each cell in a grid object.
    /// </summary>
    /// <remarks>In order for this class to work properly any <see cref="GridObject"/> this is attached to must be instantiated first.</remarks>
    [Serializable]
    public class GridObjectCellDetails
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private GridObject owningGridObject;
        private CellDetails[,] cellDetails;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        [Tooltip("Starts at bottom left of grid, goes rightward then up. So in a 2x3 example, it goes (0,0) -> (0, 1) -> (0,2) -> (1,0) -> (1,1)...")]
        [SerializeField] private List<CellDetails> details = new();

        /// <summary>
        /// Gets a list of every Cell Detail in this <see cref="GridObject"/>.
        /// </summary>
        public List<CellDetails> AllCellDetails { get { return details; } }

        /// <summary>
        /// Initializes the <see cref="GridObjectCellDetails"/> using a <see cref="GridObject"/>.
        /// </summary>
        /// <remarks>
        /// Because <see cref="GridObjectCellDetails"/> is not a <see cref="MonoBehaviour"/> call this method to correctly set up the object at runtime.
        /// </remarks>
        /// <param name="gridObject">The <see cref="GridObject"/> that contains relevant information.</param>
        public void Initialize(GridObject gridObject)
        {
            owningGridObject = gridObject;
            GridDimensions gridDimensions = gridObject.GetDimensions();

            cellDetails ??= new CellDetails[gridDimensions.X, gridDimensions.Z];
            details ??= new();

            bool populateDetailsFromList = details.Count != 0;
            if (populateDetailsFromList && details.Count != gridDimensions.X * gridDimensions.Z)
            {
                populateDetailsFromList = false;
                Debug.LogError($"The List of CellDetails populated in the list is not equal to the dimensions of the GridObject. " +
                    $"In this scenario we are unsure of what should occur to the missing cells. None of the cells will be populated as a result.");
            }

            for (int row = 0; row < gridDimensions.Z; row++)
            {
                for (int col = 0; col < gridDimensions.X; col++)
                {
                    cellDetails[col, row] = populateDetailsFromList ? details[col + row] : new CellDetails();
                }
            }
        }

        /// <summary>
        /// Gets the details of the cell at a local position of the grid object.
        /// </summary>
        /// <remarks>
        /// For example, (0,0) on the grid object does not mean the grid object is at (0,0) on its containing grid.
        /// <para>
        /// This method also keeps in mind the orientation of the object. So if its rotated 90 degrees, (0,0) is no longer at the bottom left, but the top left instead.
        /// Doing this allows the <see cref="GridObject"/> be confident that it always is referring to the same cell even as it rotates.
        /// </para>
        /// </remarks>
        /// <returns>The CellDetails that exist at that coordinate.</returns>
        public CellDetails? GetDetailFromGridObjectWithLocalCoordinate(Vector3Int localObjectCoordinates)
        {
            if (!ValidateCoordinatesAreInLocalGrid(localObjectCoordinates))
            {
                return null;
            }

            int rows = cellDetails.GetLength(0);
            int cols = cellDetails.GetLength(1);

            // TODO: verify
            switch (owningGridObject.ObjectOriginPosition)
            {
                case OriginPosition.BottomLeft:
                    return cellDetails[localObjectCoordinates.z, localObjectCoordinates.x];
                case OriginPosition.TopLeft:
                    return cellDetails[localObjectCoordinates.x, rows - 1 - localObjectCoordinates.z];
                case OriginPosition.TopRight:
                    return cellDetails[rows - 1 - localObjectCoordinates.z, cols - 1 - localObjectCoordinates.x];
                case OriginPosition.BottomRight:
                    return cellDetails[cols - 1 - localObjectCoordinates.x, localObjectCoordinates.z];
                default:
                    Debug.LogWarning($"Unkown position `{owningGridObject.ObjectOriginPosition}` when finding CellDetails.");
                    return null;
            }
        }

        /// <summary>
        /// Assigns the <see cref="CellDetails"/> to the correct cells 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="startingCellCoordinate"></param>
        /// <param name="grid"></param>
        public void AssignGridDetailsToCells<GridCellT, T>(Vector3Int startingCellCoordinate, Grid<GridCellT, T> grid) where GridCellT : GridCell<T>
                                                                                                                       where T : GridObject
        {
            Action<GridCellT, T, Vector3Int> assignCellDetailsAction = (cell, gridObject, localCoords) => 
            {
                cell.CellDetails = cellDetails[localCoords.x, localCoords.z];
                cellDetails[localCoords.x, localCoords.z].AssignCellToMonobehaviorDetails(cell);
            };

            grid.ExecuteActionOnGridCellsBasedOnObjectAndOrientation(assignCellDetailsAction, (T)owningGridObject, startingCellCoordinate);
        }

        /// <summary>
        /// Assigns the <see cref="CellDetails"/> to the correct cells 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="startingCellCoordinate"></param>
        /// <param name="grid"></param>
        public void UnassignGridDetailsToCells<GridCellT, T>(Vector3Int startingCellCoordinate, Grid<GridCellT, T> grid) where GridCellT : GridCell<T>
                                                                                                                         where T : GridObject
        {
            Action<GridCellT, T, Vector3Int> assignCellDetailsAction = (cell, _, localCoords) =>
            {
                cell.CellDetails = cellDetails[localCoords.x, localCoords.z];
                cellDetails[localCoords.x, localCoords.z].UnassignCellOnMonobehaviorDetails();
            };

            grid.ExecuteActionOnGridCellsBasedOnObjectAndOrientation(assignCellDetailsAction, (T)owningGridObject, startingCellCoordinate);
        }

        // Implement getting of particular cell detail
        public bool TryGetDetailAtLocalCoordinate<T>(Vector3Int coordinates, out T? cellDetail) where T : class
        {
            cellDetail = null;

            CellDetails? cellDetails = GetDetailFromGridObjectWithLocalCoordinate(coordinates);

            if(cellDetails == null) { return false; }

            return cellDetails.TryGetDetail(out cellDetail);
        }

        /// <summary>
        /// Checks that given coordinates are on the given <see cref="GridObject"/>.
        /// </summary>
        /// <remarks>Since <see cref="GridObject"/> have dimensions they are considered local grids.</remarks>
        /// <param name="coordinates">Coordinates on the <see cref="GridObject"/></param>
        /// <returns>True if the <paramref name="coordinates"/> fit on the <see cref="GridObject"/>.</returns>
        private bool ValidateCoordinatesAreInLocalGrid(Vector3Int coordinates)
        {
            GridDimensions dimensions = owningGridObject.GetDimensions();
            if (coordinates.x > dimensions.X - 1|| coordinates.x < 0 || coordinates.z > dimensions.Z - 1 || coordinates.z < 0)
            {
                return false;
            }

            return true;
        }

        public void OnPlaced()
        {
            AllCellDetails.ForEach(cell => cell.TriggerDetailsOnPlace(owningGridObject));
        }

        public void OnRemoved()
        {
            AllCellDetails.ForEach(cell => cell.TriggerDetailsOnGridRemoval(owningGridObject));
        }
    }
}
