using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Venwin.Utilities;

#nullable enable

namespace Venwin.Grid
{
    public class Grid<T> where T : GridObject
    {
        // Constructor Properties
        public int CellSize { get; private set; } = 1;
        public LayerMask GridLayer { get; private set; }
        public Transform Transform { get; private set; }
        public Mesh GameObjectMesh { get; private set; }

        // Runtime fields
        public GridCell<T>[,] GridCells { get; private set; }
        private Vector3 scaledBounds;
        private Vector3 bottomLeftCorner; // Used to determine world space start
        
        public int RowCount { get; private set; }
        public int ColumnCount { get; private set; }

        public Grid(Transform transform, Mesh mesh, int cellSize, LayerMask gridLayer)
        {
            CellSize = cellSize;
            GridLayer = gridLayer;
            Transform = transform;
            GameObjectMesh = mesh;
            GridCells = new GridCell<T>[0, 0];
            scaledBounds = Vector3.Scale(mesh.bounds.size, transform.lossyScale);

            Vector3 center = transform.position;
            Vector3 halfSize = new Vector3(scaledBounds.x / 2, 0, scaledBounds.z / 2);

            bottomLeftCorner = center - halfSize;

            CreateGridCells();
            DebugDrawGridLines();
            DebugCornerCoordinates();
        }

        /// <summary>
        /// Creates a grid from another grid. Allows duplicating grid features.
        /// </summary>
        /// <param name="grid">Grid to copy from</param>
        public Grid(Grid<T> grid)
            : this(grid.Transform, grid.GameObjectMesh, grid.CellSize, grid.GridLayer)
        {
        }

        /// <summary>
        /// Gets the coordinates of a cell from a point.
        /// </summary>
        /// <remarks>Assumes object's origin starts at 0,0</remarks>
        /// <param name="point">Point to compare to a cell. Will grab the closest cell to the point going to the cell's bottom left corner.</param>
        /// <returns>The cell's coordinates if its a valid point on the grid, else <see cref="Grid.InvalidCell"/>.</returns>
        public Vector3Int GetCellCoordinatesFromWorldSpace(Vector3 point)
        {
            int x = Mathf.FloorToInt(point.x - bottomLeftCorner.x) / CellSize;
            int z = Mathf.FloorToInt(point.z - bottomLeftCorner.z) / CellSize;

            if (x < 0 || x >= ColumnCount || z < 0 || z >= RowCount)
            {
                return Grid.InvalidCell;
            }

            return new Vector3Int(x, 0, z);
        }

        /// <summary>
        /// Gets a <see cref="GridCell{T}"/> from the grid from a world space point.
        /// </summary>
        /// <param name="point">Point in world space that is on the grid that can be mapped to the grid coordinate.</param>
        /// <returns>The <see cref="GridCell{T}"/> in that worldspace, else null if the <paramref name="point"/> is not on the grid.</returns>
        public GridCell<T>? GetCellFromWorldSpace(Vector3 point)
        {
            Vector3Int cellCoordinates = GetCellCoordinatesFromWorldSpace(point);

            if(cellCoordinates == Grid.InvalidCell) { return null; }

            return GridCells[cellCoordinates.x, cellCoordinates.z];
        }

        public GridCell<T>? GetCellFromGridCoordiantes(Vector3Int gridCoordinate)
        {
            if (!IsValidCellCoordinate(gridCoordinate))
            {
                return null;
            }

            return GridCells[gridCoordinate.x, gridCoordinate.z];
        }

        /// <summary>
        /// Checks whether we can place an object with the given dimensions (ie are there cells on the grid to take the entire object)
        /// </summary>
        /// <param name="cellStart">Start of the cell to place the <see cref="GridObject"/>.</param>
        /// <param name="dimensions"><see cref="GridDimensions"/> of the <see cref="GridObject"/>.</param>
        /// <param name="originPosition"><see cref="GridObject"/>'s origin position.</param>
        /// <returns>True if there are cells that can fit the dimensions, else false.</returns>
        /// <exception cref="NotImplementedException">Thrown if invalid <see cref="OriginPosition"/>.</exception>
        public bool AreAllGridCellsPlaceable(Vector3Int cellStart, GridDimensions dimensions, OriginPosition originPosition = OriginPosition.BottomLeft)
        {
            if (!IsValidCellCoordinate(cellStart))
            {
                Debug.LogWarning($"Invalid start grid cell was given in {nameof(AreAllGridCellsPlaceable)}");
                return false;
            }

            switch (originPosition)
            {
                case OriginPosition.BottomLeft:
                    return DetermineIfWithinBounds(cellStart, dimensions.X, dimensions.Z);
                case OriginPosition.TopLeft:
                    return DetermineIfWithinBounds(cellStart, dimensions.Z, -dimensions.X);
                case OriginPosition.BottomRight:
                    return DetermineIfWithinBounds(cellStart, -dimensions.Z, dimensions.X);
                case OriginPosition.TopRight:
                    return DetermineIfWithinBounds(cellStart, -dimensions.X, -dimensions.Z);
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Checks if the grid can fit the <paramref name="dimensions"/> on the grid as well as if the <see cref="GridCell{T}"/>s are marked as avaiable. 
        /// </summary>
        /// <param name="cellStart">The starting position of the <see cref="GridObject"/>.</param>
        /// <param name="dimensions">The <see cref="GridObject"/>'s dimensions.</param>
        /// <param name="objectOriginPosition">The <see cref="GridObject"/>'s <see cref="OriginPosition"/>.</param>
        /// <returns>True if there are cells that can hold the object and are marked as available. Else false.</returns>
        /// <exception cref="NotImplementedException">Thrown if invalid <see cref="OriginPosition"/>.</exception>
        public bool AreAllGridCellsAvailable(Vector3Int cellStart, T gridObject)
        {
            if (!IsValidCellCoordinate(cellStart))
            {
                Debug.LogWarning($"Invalid start grid cell was given in {nameof(AreAllGridCellsAvailable)}");
                return false;
            }

            GridDimensions dimensions = gridObject.GetDimensions();

            switch (gridObject.ObjectOriginPosition)
            {
                case OriginPosition.BottomLeft:
                    return DetermineIfWithinBoundsAndAvailable(cellStart, gridObject, dimensions.X, dimensions.Z);
                case OriginPosition.TopLeft:
                    return DetermineIfWithinBoundsAndAvailable(cellStart, gridObject, dimensions.Z, -dimensions.X);
                case OriginPosition.BottomRight:
                    return DetermineIfWithinBoundsAndAvailable(cellStart, gridObject, -dimensions.Z, dimensions.X);
                case OriginPosition.TopRight:
                    return DetermineIfWithinBoundsAndAvailable(cellStart, gridObject, -dimensions.X, -dimensions.Z);
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets all the objects in surrounding grids of a given cell coordinate.
        /// </summary>
        /// <param name="cellCoordinate">Coordinate of the cell to search around.</param>
        /// <returns>A list of nullable grid elements starting from the north and going clockwise.</returns>
        public List<T?> GetAllElementsOnGridAroundCell(Vector3Int cellCoordinate)
        {
            List<T?> gridElements = new();

            if (!IsValidCellCoordinate(cellCoordinate))
            {
                Debug.LogWarning($"Trying to get all grid elements around a cell coordinate but the given cell coordinate is invalid: {cellCoordinate}");
               return gridElements;
            }

            // North
            if(IsValidCellCoordinate(cellCoordinate + new Vector3Int(0, 0, 1)))
            {
                gridElements.Add(GridCells[cellCoordinate.z + 1, cellCoordinate.x].CurrentObject);
            } else { gridElements.Add(null); }

            // East
            if (IsValidCellCoordinate(cellCoordinate + new Vector3Int(1, 0, 0)))
            {
                gridElements.Add(GridCells[cellCoordinate.z, cellCoordinate.x + 1].CurrentObject);
            } else { gridElements.Add(null); }

            // South
            if (IsValidCellCoordinate(cellCoordinate + new Vector3Int(0, 0, 1)))
            {
                gridElements.Add(GridCells[cellCoordinate.z - 1, cellCoordinate.x].CurrentObject);
            } else { gridElements.Add(null); }

            // West
            if (IsValidCellCoordinate(cellCoordinate + new Vector3Int(0, 0, 1)))
            {
                gridElements.Add(GridCells[cellCoordinate.z, cellCoordinate.x - 1].CurrentObject);
            } else { gridElements.Add(null); }

            return gridElements;
        }


        // TODO VERIFY, I THINK WE NEED TO ACTUALLY CALL GridObjectCellDetails TO HANDLE ROTATION AS WELL
        public List<TDetail?> GetDetailFromSurroundingCells<TDetail>(Vector3Int cellCoordinate) where TDetail : class
        {
            List<TDetail?> gridDetails = new();

            if (!IsValidCellCoordinate(cellCoordinate))
            {
                Debug.LogWarning($"Trying to get all grid elements around a cell coordinate but the given cell coordinate is invalid: {cellCoordinate}");
                return gridDetails;
            }

            // North
            if (IsValidCellCoordinate(cellCoordinate + new Vector3Int(0, 0, 1)))
            {
                TDetail? northDetail = null;
                GridCells[cellCoordinate.x, cellCoordinate.z + 1].CellDetails?.TryGetDetail<TDetail>(out northDetail);
                gridDetails.Add(northDetail);
            }
            else { gridDetails.Add(null); }

            // East
            if (IsValidCellCoordinate(cellCoordinate + new Vector3Int(1, 0, 0)))
            {
                TDetail? eastDetail = null;
                GridCells[cellCoordinate.x + 1, cellCoordinate.z].CellDetails?.TryGetDetail<TDetail>(out eastDetail);
                gridDetails.Add(eastDetail);
            }
            else { gridDetails.Add(null); }

            // South
            if (IsValidCellCoordinate(cellCoordinate + new Vector3Int(0, 0, 1)))
            {
                TDetail? southDetail = null;
                GridCells[cellCoordinate.x, cellCoordinate.z - 1].CellDetails?.TryGetDetail<TDetail>(out southDetail);
                gridDetails.Add(southDetail);
            }
            else { gridDetails.Add(null); }

            // West
            if (IsValidCellCoordinate(cellCoordinate + new Vector3Int(0, 0, 1)))
            {
                TDetail? westDetail = null;
                GridCells[cellCoordinate.x - 1, cellCoordinate.z].CellDetails?.TryGetDetail<TDetail>(out westDetail);
                gridDetails.Add(westDetail);
            }
            else { gridDetails.Add(null); }

            return gridDetails;
        }

        /// <summary>
        /// Tries to place the object on the grid. If its doesn't fit on the grid its not placed.
        /// </summary>
        /// <param name="gridObject"></param>
        /// <param name="startingCellCoordinate"></param>
        /// <param name="instantiatedGridObject"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public bool TryPlaceGridObject(T gridObject, Vector3Int startingCellCoordinate, out T? instantiatedGridObject, Transform? parent = null, bool forcePlacement = false)
        {
            instantiatedGridObject = null;
            GridDimensions dimensions = gridObject.GetDimensions();

            if (forcePlacement)
            {
                if (!AreAllGridCellsPlaceable(startingCellCoordinate, dimensions, gridObject.ObjectOriginPosition)) { return false; }
            }
            else
            {
                if(!AreAllGridCellsAvailable(startingCellCoordinate, gridObject)) { return false; }
            }

            instantiatedGridObject = UnityEngine.Object.Instantiate(gridObject, GetWorldPositionFromCellAndRotation(startingCellCoordinate, gridObject), gridObject.GetQuaternionBasedOnOriginPosition());
            if (parent != null)
            {
                instantiatedGridObject.transform.parent = parent.transform;
                //TransformUtilities.IgnoreParentScale(instantiatedGridObject.transform);
            }

            Action<GridCell<T>, T, Vector3Int> addAction = (GridCell<T> gridCell, T gridObject1, Vector3Int _) => { gridCell.AddObject(gridObject1); };
            ExecuteActionOnGridCellsBasedOnObjectAndOrientation(addAction, gridObject, startingCellCoordinate);

            instantiatedGridObject.GridObjectCellDetails.AssignGridDetailsToCells(startingCellCoordinate, this);

            return true;
        }

        /// <summary>
        /// Executes an action based on the grid object and passes the correct grid coordinates based on the <see cref="GridObject"/>'s orientation.
        /// </summary>
        ///
        /// <remarks>
        /// <paramref name="action"/> will be executed on every cell on the grid based on the dimensions of the <see cref="GridObject"/>. It takes the following:
        ///<list type = "bullet" >
        /// <item><see cref="GridCell{T}"/> - Current cell that the <paramref name="action"/> will be executed on.</item>
        /// <item><typeparamref name="T"/> - The <see cref="GridObject"/> that may be relevant for the cell.</item>
        /// <item><see cref="Vector3Int"/> - The local cell coordinate of the <see cref="GridObject"/>.</item>
        /// </list>
        /// </remarks>
        /// 
        /// <param name="action">
        /// <list type="bullet">
        /// <item><see cref="GridCell{T}"/>Current cell that the action will be executed on.</item>
        /// <item><see cref="GridObject"/>The grid object that may be relevant for the cell.</item>
        /// <item>The local cell coordinate of the <see cref="GridObject"/>.</item>
        /// </list>
        /// </param>
        /// <param name="gridObject">The <see cref="GridObject"/> that is relevant for orientation and dimensions.</param>
        /// <param name="startingCell">The starting cell for the operation.</param>
        public void ExecuteActionOnGridCellsBasedOnObjectAndOrientation(Action<GridCell<T>, T, Vector3Int> action,
                                                                            T gridObject,
                                                                            Vector3Int startingCell)
        {
            try
            {
                GridDimensions dimensions = gridObject.GetDimensions();
                switch (gridObject.ObjectOriginPosition)
                {
                    case OriginPosition.BottomLeft:
                        for (int col = 0; col < dimensions.X; col++)
                        {
                            for (int row = 0; row < dimensions.Z; row++)
                            {
                                GridCell<T> cell = GridCells[startingCell.x + col, startingCell.z + row];

                                // Executes action
                                action(cell, gridObject, new Vector3Int(col, 0, row));
                            }
                        }
                        break;

                    case OriginPosition.TopLeft:
                        for (int col = 0; col < dimensions.X; col++)
                        {
                            for (int row = 0; row < dimensions.Z; row++)
                            {
                                GridCell<T> cell = GridCells[startingCell.x + row, startingCell.z - col];

                                action(cell, gridObject, new Vector3Int(row, 0, col));
                            }
                        }
                        break;

                    case OriginPosition.TopRight:
                        for (int col = 0; col < dimensions.X; col++)
                        {
                            for (int row = 0; row < dimensions.Z; row++)
                            {
                                GridCell<T> cell = GridCells[startingCell.x - col, startingCell.z - row];

                                action(cell, gridObject, new Vector3Int(row, 0, col));
                            }
                        }
                        break;

                    case OriginPosition.BottomRight:
                        for (int col = 0; col < dimensions.X; col++)
                        {
                            for (int row = 0; row < dimensions.Z; row++)
                            {
                                GridCell<T> cell = GridCells[startingCell.x - row, startingCell.z + col];

                                action(cell, gridObject, new Vector3Int(row, 0, col));
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"An error occured when trying to execute an anonymous action on the grid based on grid object and orientation: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the correct world position a <see cref="GridObject"/> should be placed in world space. Accounts for rotation and cell coordinate.
        /// </summary>
        /// <param name="cellCoordinate">Coordinate of a specific cell on this grid.</param>
        /// <param name="gridObject">Object attempting to be instantiated.</param>
        /// <returns>A world space position that considers a cell's world space and rotational offset</returns>
        private Vector3 GetWorldPositionFromCellAndRotation(Vector3Int cellCoordinate, T gridObject)
        {
            Vector3 worldSpace = GetCellFromGridCoordiantes(cellCoordinate)!.WorldSpaceCoordinates;

            switch (gridObject.ObjectOriginPosition)
            {
                case OriginPosition.BottomLeft:
                    return worldSpace;
                case OriginPosition.TopLeft:
                    return worldSpace + new Vector3(0, 0, CellSize);
                case OriginPosition.TopRight:
                    return worldSpace + new Vector3(CellSize, 0, CellSize);
                case OriginPosition.BottomRight:
                    return worldSpace + new Vector3(CellSize, 0, 0);
                default: return worldSpace;
            }

        }

        #region Debug Helpers

        public void DebugClickLogs()
        {
            ClickInformation clickInfo = CursorUtilities.CursorToWorldPosition(GridLayer);
            if (clickInfo.DidHit)
            {
                Debug.Log($"Hit Point: {clickInfo.Point}");

                Vector3Int cellCoords = GetCellCoordinatesFromWorldSpace(clickInfo.Point);
                Debug.Log(cellCoords);
                Debug.Log(GridCells[cellCoords.x, cellCoords.z].IsAvailable);
            }
        }

        public void DebugDrawGridLines()
        {
            for (int row = 0; row < RowCount + 1; row++)
            {
                Debug.DrawRay(new Vector3(0, 0, row * CellSize) + bottomLeftCorner, new Vector3(ColumnCount * CellSize, 0, 0), Color.red, Mathf.Infinity);
            }

            for (int col = 0; col < ColumnCount + 1; col++)
            {
                Debug.DrawRay(new Vector3(col * CellSize, 0, 0) + bottomLeftCorner, new Vector3(0, 0, RowCount * CellSize), Color.green, Mathf.Infinity);
            }
        }

        public void DebugCornerCoordinates()
        {
            Vector3 center = Transform.position;
            Vector3 halfSize = new Vector3(scaledBounds.x / 2, 0, scaledBounds.z / 2);

            Vector3 bottomLeft = center - halfSize;
            Vector3 bottomRight = center + new Vector3(halfSize.x, 0, -halfSize.z);
            Vector3 topRight = center + halfSize;
            Vector3 topLeft = center + new Vector3(-halfSize.x, 0, halfSize.z);

            Debug.Log("Center: " + center);
            Debug.Log("Half: " + halfSize);
            Debug.Log("Top Left Corner: " + topLeft);
            Debug.Log("Top Right Corner: " + topRight);
            Debug.Log("Bottom Left Corner: " + bottomLeft);
            Debug.Log("Bottom Right Corner: " + bottomRight);
        }

        #endregion

        /// <summary>
        /// Creates the grid cells
        /// </summary>
        private void CreateGridCells()
        {
            ColumnCount = Mathf.FloorToInt(scaledBounds.x / CellSize);
            RowCount = Mathf.FloorToInt(scaledBounds.z / CellSize);

            GridCells = new GridCell<T>[ColumnCount, RowCount];

            for (int col = 0; col < ColumnCount; col++)
            {
                for (int row = 0; row < RowCount; row++)
                {
                    Vector3 worldSpaceCoordinates = new Vector3(col * CellSize + bottomLeftCorner.x, 0, row * CellSize + bottomLeftCorner.z);
                    GridCells[col, row] = new GridCell<T>(CellSize, new Vector3Int(col, 0, row), worldSpaceCoordinates);
                }
            }
        }

        /// <summary>
        /// Checks if a given coordinate is valid and actually has a cell.
        /// </summary>
        /// <param name="cellCoordinate">Coordinate to verify</param>
        /// <returns>True if the coordinate is on the grid.</returns>
        private bool IsValidCellCoordinate(Vector3Int cellCoordinate)
        {
            if (cellCoordinate.x < 0 || cellCoordinate.x > ColumnCount || cellCoordinate.z < 0 || cellCoordinate.z > RowCount)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Determines if given x and z coordinates are going to keep an item inside a grid.
        /// </summary>
        /// <remarks>
        /// Cell start must be verified as valid before using helper function.
        /// X and Z can be positive or negative to help with orientation/direction of item.
        /// </remarks>
        /// <param name="verifiedCellStart">Where the starting cell is located</param>
        /// <param name="columnLength">The length along the columns</param>
        /// <param name="rowLength">The length along the rows</param>
        /// <returns>True if the dimensions would fit, else false.</returns>
        private bool DetermineIfWithinBounds(Vector3Int verifiedCellStart, int columnLength, int rowLength)
        {
            // Head one step closer to 0, because we don't want to count the starting cell.
            columnLength = MathUtilities.MeanReversion(columnLength);
            rowLength = MathUtilities.MeanReversion(rowLength);

            if (verifiedCellStart.x + columnLength < ColumnCount &&
                verifiedCellStart.x + columnLength >= 0 &&
                verifiedCellStart.z + rowLength < RowCount &&
                verifiedCellStart.z + rowLength >= 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool DetermineIfWithinBoundsAndAvailable(Vector3Int verifiedCellStart, T gridObject, int columnLength, int rowLength)
        {
            bool isAvailable = true;

            if(!DetermineIfWithinBounds(verifiedCellStart, columnLength, rowLength)) { return false; }

            Action<GridCell<T>, T, Vector3Int> availabilityCheck = (cell, gridObject, localCoords) =>
            {
                if(!cell.IsAvailable) { isAvailable = false; }
            };

            ExecuteActionOnGridCellsBasedOnObjectAndOrientation(availabilityCheck, gridObject, verifiedCellStart);

            return isAvailable;
        }
    }

    /// <summary>
    /// Enum that outlines where the origin of a grid item is considered to start. Defaults to the bottom left corner of the grid.
    /// </summary>
    public enum OriginPosition
    {
        BottomLeft = 0,
        TopLeft = 1,
        TopRight = 2,
        BottomRight = 3
    }

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
