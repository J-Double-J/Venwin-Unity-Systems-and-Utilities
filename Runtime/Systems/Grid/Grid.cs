using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Venwin.Utilities;

#nullable enable

namespace Venwin.Grid
{
    /// <summary>
    /// Non-generic implementation of a grid. Used when the type of object stored doesn't matter.
    /// </summary>
    public abstract class Grid
    {
        #region Declarations

        public static Vector3Int InvalidCell = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);

        #endregion

        #region Properties

        // Constructor Properties
        public int CellSize { get; protected set; } = 1;
        public LayerMask GridLayer { get; protected set; }
        public Transform Transform { get; protected set; }
        public Mesh GameObjectMesh { get; protected set; }

        public int RowCount { get; private set; }
        public int ColumnCount { get; private set; }
        public int YAxisMax { get; private set; }

        public Dictionary<Vector3Int, GridCell> GridCells { get; protected set; }

        [Tooltip("This property configures the grid to be able to use pathfinding.")]
        public bool GridIsNavigatable { get; } = true;

        protected Vector3 ScaledBounds { get; set; }

        /// <summary>
        /// Used to determine world space start of the grid.
        /// </summary>
        protected Vector3 BottomLeftCorner { get; set; }

        /// <summary>
        /// Function that is called to create each GridCell if specified. Allows creation of custom <see cref="GridCell"/>
        /// </summary>
        protected Func<Grid, int, Vector3Int, Vector3, GridCell>? CellCreationCallback { get; set; } = null;

        /// <summary>
        /// Enables drawing debug lines in the grid.
        /// </summary>
        public bool DrawDebug { get; set; } = true;

        /// <summary>
        /// Enables printing debugging messages to the console.
        /// </summary>
        public bool PrintDebug { get; set; } = false;

        #endregion

        #region Events

        public event EventHandler<GridPlacementEventArgs>? OnGridObjectPlaced;

        #endregion

        public Grid(Transform transform, Mesh mesh, int cellSize, int yAxisMax, LayerMask gridLayer)
            : this(transform, mesh, cellSize, yAxisMax, gridLayer, null)
        {
        }

        /// <summary>
        /// Grid with a callback that allows custom cell creation.
        /// </summary>
        /// <param name="transform">The transform this grid is on.</param>
        /// <param name="mesh">The bounding mesh for the grid. Used to determine how far to draw grid.</param>
        /// <param name="cellSize">The size of each individaul cell.</param>
        /// <param name="yAxisMax">The maximum number of y-layers to the grid. Use 0 for a "one layer" or 2D grid.</param>
        /// <param name="gridLayer">Layer the grid resides on.</param>
        /// <param name="callback">Function that takes in parameters for a a grid cell to create custom cells.</param>
        public Grid(Transform transform, Mesh mesh, int cellSize, int yAxisMax, LayerMask gridLayer, Func<Grid, int, Vector3Int, Vector3, GridCell>? callback)
        {
            if(yAxisMax < 0)
            {
                throw new ArgumentException($"{nameof(Grid)} cannot have a {nameof(yAxisMax)} value of less than 0");
            }

            Transform = transform;
            GameObjectMesh = mesh;
            CellSize = cellSize;
            GridLayer = gridLayer;
            GridCells = new();

            ScaledBounds = Vector3.Scale(mesh.bounds.size, transform.lossyScale);
            Vector3 center = transform.position;
            Vector3 halfSize = new Vector3(ScaledBounds.x / 2, 0, ScaledBounds.z / 2); // GRID TODO: Does BottomLeft need to be projected?
            BottomLeftCorner = center - halfSize;

            ColumnCount = Mathf.FloorToInt(ScaledBounds.x / CellSize);
            RowCount = Mathf.FloorToInt(ScaledBounds.z / CellSize);
            YAxisMax = yAxisMax + 1;
            
            CellCreationCallback = callback;

            CreateGridCells();

            if (GridIsNavigatable)
            {
                ConfigureGridCellsForNavigation();
            }
        }


        #region Abstracts

        /// <summary>
        /// Creates grid cells.
        /// </summary>
        /// <remarks>Concrete implementors must fill the <see cref="GridCells"/> matrix via this method.</remarks>
        protected abstract void CreateGridCells();

        /// <summary>
        /// Configures the cells in the grid to be ready for navigation.
        /// </summary>
        /// <remarks>
        /// This method is only called if GridIsNavigatable is set to true.<br/>
        /// This is where you can configure special navigation rules between grid cells.
        /// </remarks>
        protected abstract void ConfigureGridCellsForNavigation();

        #endregion

        #region Translating to and from World Space and Grid Space Coordinates

    /// <summary>
    /// Gets the coordinates of a cell from a point.
    /// </summary>
    /// <remarks>Assumes object's origin starts at 0,0</remarks>
    /// <param name="point">Point to compare to a cell. Will grab the closest cell to the point going to the cell's bottom left corner.</param>
    /// <returns>The cell's coordinates if its a valid point on the grid, else <see cref="Grid.InvalidCell"/>.</returns>
        public Vector3Int GetCellCoordinatesFromWorldSpace(Vector3 point)
        {
            float roundedY = Mathf.Round(point.y * 10f) / 10f; // Round to the nearest tenth

            int x = Mathf.FloorToInt(point.x - BottomLeftCorner.x) / CellSize;
            int z = Mathf.FloorToInt(point.z - BottomLeftCorner.z) / CellSize;
            int y = Mathf.FloorToInt(roundedY - BottomLeftCorner.y) / CellSize;

            if (x < 0 || x >= ColumnCount || z < 0 || z >= RowCount || y < 0 || y >= YAxisMax)
            {
                return InvalidCell;
            }

            return new Vector3Int(x, y, z);
        }

        /// <summary>
        /// Gets a <see cref="GridCell"/> from the grid from a world space point.
        /// </summary>
        /// <param name="worldSpacePoint">Point in world space that is on the grid that can be mapped to the grid coordinate.</param>
        /// <returns>The <see cref="GridCell"/> in that worldspace, else null if the <paramref name="worldSpacePoint"/> is not on the grid.</returns>
        public GridCell? GetCellFromWorldSpace(Vector3 worldSpacePoint)
        {
            Vector3Int cellCoordinates = GetCellCoordinatesFromWorldSpace(worldSpacePoint);

            if (cellCoordinates == InvalidCell) { return null; }

            GridCells.TryGetValue(cellCoordinates, out GridCell? cell);
            return cell;
        }

        /// <summary>
        /// Gets a <see cref="GridCell"/> from the grid from a grid coordinate.
        /// </summary>
        /// <param name="gridCoordinate">A coordinate that is on the grid.</param>
        /// <returns>A grid cell if the coordinate was valid.</returns>
        public GridCell? GetCellFromGridCoordinates(Vector3Int gridCoordinate)
        {
            if (!IsValidCellCoordinate(gridCoordinate))
            {
                return null;
            }

            GridCells.TryGetValue(gridCoordinate, out GridCell? cell);
            return cell;
        }

        /// <summary>
        /// Gets the correct world position a <see cref="GridObject"/> should be placed in world space. Accounts for rotation and cell coordinate.
        /// </summary>
        /// <param name="cellCoordinate">Coordinate of a specific cell on this grid.</param>
        /// <param name="gridObject">Object containing the rotation information.</param>
        /// <returns>A world space position that considers a cell's world space and rotational offset</returns>
        public Vector3 GetWorldPositionFromCellAndRotation(Vector3Int cellCoordinate, GridObject gridObject)
        {
            return GetWorldPositionFromCellAndRotation(cellCoordinate, gridObject.ObjectOriginPosition);
        }

        /// <summary>
        /// Gets the correct world position from a cell coordinate in relation to an origin position.
        /// </summary>
        /// <param name="cellCoordinate">Coordinate of a specific cell on this grid.</param>
        /// <param name="originPosition">Origin postion to consider.</param>
        /// <returns>A world space position that considers a cell's world space and rotational offset</returns>
        public Vector3 GetWorldPositionFromCellAndRotation(Vector3Int cellCoordinate, OriginPosition originPosition)
        {
            if (!IsValidCellCoordinate(cellCoordinate)) { throw new InvalidOperationException($"Cannot use an invalid cell coordinate for {nameof(GetWorldPositionFromCellAndRotation)}"); }

            Vector3 worldSpace = GetCellFromGridCoordinates(cellCoordinate)!.WorldSpaceCoordinates;

            switch (originPosition)
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

        #endregion

        #region Availability Checks

        /// <summary>
        /// Checks if the grid can fit the <see cref="GridObject"/>'s dimensions. on the grid as well as if the <see cref="GridCell"/>s are marked as avaiable. 
        /// </summary>
        /// <param name="cellStart">The starting position of the <see cref="GridObject"/>.</param>
        /// <param name="gridObject">Grid object to use to check if the other grid cells are available.</param>
        /// <returns>True if there are cells that can hold the object and are marked as available. Else false.</returns>
        /// <exception cref="NotImplementedException">Thrown if invalid <see cref="GridObject.ObjectOriginPosition"/>.</exception>
        public bool AreAllGridCellsAvailable(Vector3Int cellStart, GridObject gridObject)
        {
            if (!IsValidCellCoordinate(cellStart))
            {
                Debug.LogWarning($"Invalid start grid cell was given in {nameof(AreAllGridCellsAvailable)}");
                return false;
            }

            GridDimensions dimensions = gridObject.GetDimensions();

            return gridObject.ObjectOriginPosition switch
            {
                OriginPosition.BottomLeft => DetermineIfWithinBoundsAndAvailable(cellStart, gridObject, dimensions.X, dimensions.Y, dimensions.Z),
                OriginPosition.TopLeft => DetermineIfWithinBoundsAndAvailable(cellStart, gridObject, dimensions.Z, dimensions.Y, -dimensions.X),
                OriginPosition.BottomRight => DetermineIfWithinBoundsAndAvailable(cellStart, gridObject, -dimensions.Z, dimensions.Y, dimensions.X),
                OriginPosition.TopRight => DetermineIfWithinBoundsAndAvailable(cellStart, gridObject, -dimensions.X, dimensions.Y, -dimensions.Z),
                _ => throw new NotImplementedException(),
            };
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
                    return DetermineIfWithinBounds(cellStart, dimensions.X, dimensions.Y, dimensions.Z);
                case OriginPosition.TopLeft:
                    return DetermineIfWithinBounds(cellStart, dimensions.Z, dimensions.Y, -dimensions.X);
                case OriginPosition.BottomRight:
                    return DetermineIfWithinBounds(cellStart, -dimensions.Z, dimensions.Y, dimensions.X);
                case OriginPosition.TopRight:
                    return DetermineIfWithinBounds(cellStart, -dimensions.X, dimensions.Y, -dimensions.Z);
                default:
                    throw new NotImplementedException();
            }
        }

        protected bool DetermineIfWithinBoundsAndAvailable(Vector3Int verifiedCellStart, GridObject gridObject, int columnLength, int yHeight, int rowLength)
        {
            bool isAvailable = true;

            if (!DetermineIfWithinBounds(verifiedCellStart, columnLength, yHeight, rowLength)) { return false; }

            Action<GridCell, GridObject, Vector3Int> availabilityCheck = (cell, gridObject, localCoords) =>
            {
                if (!cell.IsAvailable) { isAvailable = false; }
            };

            ExecuteActionOnGridCellsBasedOnObjectAndOrientation(availabilityCheck, gridObject, verifiedCellStart);

            return isAvailable;
        }

        #endregion

        /// <summary>
        /// Executes an action based on the grid object and passes the correct grid coordinates based on the <see cref="GridObject"/>'s orientation.
        /// </summary>
        ///
        /// <remarks>
        /// <paramref name="action"/> will be executed on every <em>existing</em> cell on the grid based on the dimensions of the <see cref="GridObject"/>. It takes the following:
        ///<list type = "bullet" >
        /// <item><see cref="GridCell"/> - Current cell that the <paramref name="action"/> will be executed on.</item>
        /// <item><see cref="GridObject"/>- The <see cref="GridObject"/> that may be relevant for the cell.</item>
        /// <item><see cref="Vector3Int"/> - The local cell coordinate of the <see cref="GridObject"/>.</item>
        /// </list>
        /// </remarks>
        /// 
        /// <param name="action">
        ///<list type = "bullet" >
        /// <item><see cref="GridCell"/> - Current cell that the <paramref name="action"/> will be executed on.</item>
        /// <item><see cref="GridObject"/>- The <see cref="GridObject"/> that may be relevant for the cell.</item>
        /// <item><see cref="Vector3Int"/> - The local cell coordinate of the <see cref="GridObject"/>.</item>
        /// </list>
        /// </param>
        /// <param name="gridObject">The <see cref="GridObject"/> that is relevant for orientation and dimensions.</param>
        /// <param name="startingCell">The starting cell for the operation.</param>
        public void ExecuteActionOnGridCellsBasedOnObjectAndOrientation(Action<GridCell, GridObject, Vector3Int> action,
                                                                            GridObject gridObject,
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
                            for(int yAxis = 0; yAxis < dimensions.Y; yAxis++)
                            {
                                for (int row = 0; row < dimensions.Z; row++)
                                {
                                    GridCells.TryGetValue(new Vector3Int(startingCell.x + col, startingCell.y + yAxis, startingCell.z + row), out GridCell? gridCell);

                                    if(gridCell != null)
                                    {
                                        // Executes action
                                        action(gridCell, gridObject, new Vector3Int(col, yAxis, row));
                                    }
                                }
                            }
                        }
                        break;

                    case OriginPosition.TopLeft:
                        for (int col = 0; col < dimensions.X; col++)
                        {
                            for (int yAxis = 0; yAxis < dimensions.Y; yAxis++)
                            {
                                for (int row = 0; row < dimensions.Z; row++)
                                {
                                    GridCells.TryGetValue(new Vector3Int(startingCell.x + row, startingCell.y + yAxis, startingCell.z - col), out GridCell? gridCell);

                                    if (gridCell != null)
                                    {
                                        action(gridCell, gridObject, new Vector3Int(col, yAxis, row));
                                    }
                                }
                            }
                        }
                        break;

                    case OriginPosition.TopRight:
                        for (int col = 0; col < dimensions.X; col++)
                        {
                            for (int yAxis = 0; yAxis < dimensions.Y; yAxis++)
                            {
                                for (int row = 0; row < dimensions.Z; row++)
                                {
                                    GridCells.TryGetValue(new Vector3Int(startingCell.x - col, startingCell.y + yAxis, startingCell.z - row), out GridCell? gridCell);

                                    if (gridCell != null)
                                    {
                                        action(gridCell, gridObject, new Vector3Int(col, yAxis, row));
                                    }
                                }
                            }
                        }
                        break;

                    case OriginPosition.BottomRight:
                        for (int col = 0; col < dimensions.X; col++)
                        {
                            for (int yAxis = 0; yAxis < dimensions.Y; yAxis++)
                            {
                                for (int row = 0; row < dimensions.Z; row++)
                                {
                                    GridCells.TryGetValue(new Vector3Int(startingCell.x - row, startingCell.y + yAxis, startingCell.z + col), out GridCell? gridCell);

                                    if (gridCell != null)
                                    {
                                        action(gridCell, gridObject, new Vector3Int(col, yAxis, row));
                                    }
                                }
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

        #region Cell Information Fetching
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
                GridCells.TryGetValue(new Vector3Int(cellCoordinate.x, cellCoordinate.y, cellCoordinate.z + 1), out GridCell? gridCell);
                gridCell?.CellDetails?.TryGetDetail<TDetail>(out northDetail);
                gridDetails.Add(northDetail);
            }
            else { gridDetails.Add(null); }

            // East
            if (IsValidCellCoordinate(cellCoordinate + new Vector3Int(1, 0, 0)))
            {
                TDetail? eastDetail = null;
                GridCells.TryGetValue(new Vector3Int(cellCoordinate.x + 1, cellCoordinate.y, cellCoordinate.z), out GridCell? gridCell);
                gridCell?.CellDetails?.TryGetDetail<TDetail>(out eastDetail);
                gridDetails.Add(eastDetail);
            }
            else { gridDetails.Add(null); }

            // South
            if (IsValidCellCoordinate(cellCoordinate + new Vector3Int(0, 0, 1)))
            {
                TDetail? southDetail = null;
                GridCells.TryGetValue(new Vector3Int(cellCoordinate.x, cellCoordinate.y, cellCoordinate.z - 1), out GridCell? gridCell);
                gridCell?.CellDetails?.TryGetDetail<TDetail>(out southDetail);
                gridDetails.Add(southDetail);
            }
            else { gridDetails.Add(null); }

            // West
            if (IsValidCellCoordinate(cellCoordinate + new Vector3Int(0, 0, 1)))
            {
                TDetail? westDetail = null;
                GridCells.TryGetValue(new Vector3Int(cellCoordinate.x - 1, cellCoordinate.y, cellCoordinate.z), out GridCell? gridCell);
                gridCell?.CellDetails?.TryGetDetail<TDetail>(out westDetail);
                gridDetails.Add(westDetail);
            }
            else { gridDetails.Add(null); }

            return gridDetails;
        }

        /// <summary>
        /// Gets a <see cref="GridCell"/>'s neighbor in a certain direction.
        /// </summary>
        /// <param name="startingCell">The cell to check from.</param>
        /// <param name="neighborDirection">The direction on the grid to check.</param>
        /// <param name="neighborCheck">If true, checks if the starting cell is considered a neighbor with the found cell. Otherwise skips this check.</param>
        /// <returns>The neighboring <see cref="GridCell"/> if its on the grid and considered a neighbor of <paramref name="startingCell"/>.
        /// Else null.</returns>
        public GridCell? GetCellNeighbor(GridCell startingCell, GridDirection neighborDirection, bool neighborCheck)
        {
            Vector3Int neighboringCoordinate = startingCell.GridCoordinates + neighborDirection.GetVectorFromDirection();
            if (!IsValidCellCoordinate(neighboringCoordinate))
            {
                return null;
            }

            GridCell validCell = GridCells[neighboringCoordinate];
            
            if(!neighborCheck) { return validCell; }

            // Just because the cells are physically next to each other, doesn't mean that the game considers them
            // neighbors. We have to check that they are.
            if (startingCell.Neighbors.Contains(validCell)) { return validCell; }

            return null;
        }

        #endregion

        #region Placement

        public void ObjectPlacedOnGrid(GridObject gridObject)
        {
            gridObject.OnPlaced();
            OnGridObjectPlaced?.Invoke(this, new GridPlacementEventArgs(gridObject));
        }

        #endregion

        #region Validation

        /// <summary>
        /// Checks if a given coordinate is valid and actually has a cell.
        /// </summary>
        /// <param name="cellCoordinate">Coordinate to verify</param>
        /// <returns>True if the coordinate is on the grid.</returns>
        protected bool IsValidCellCoordinate(Vector3Int cellCoordinate)
        {
            // >= is done because the origin starts (0,0) not at (1,1)
            if (cellCoordinate.x < 0 || cellCoordinate.x >= ColumnCount ||
                cellCoordinate.z < 0 || cellCoordinate.z >= RowCount ||
                cellCoordinate.y < 0 || cellCoordinate.y >= YAxisMax)
            {
                if(!GridCells.ContainsKey(cellCoordinate))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Determines if given x, y, z coordinates are going to keep an item inside a grid.
        /// </summary>
        /// <remarks>
        /// Cell start must be verified as valid before using helper function.
        /// X and Z can be positive or negative to help with orientation/direction of item.
        /// </remarks>
        /// <param name="verifiedCellStart">Where the starting cell is located</param>
        /// <param name="columnLength">The length along the columns</param>
        /// <param name="rowLength">The length along the rows</param>
        /// <returns>True if the dimensions would fit, else false.</returns>
        protected bool DetermineIfWithinBounds(Vector3Int verifiedCellStart, int columnLength, int yHeight, int rowLength)
        {
            // Head one step closer to 0, because we don't want to count the starting cell.
            columnLength = MathUtilities.MeanReversion(columnLength);
            yHeight = MathUtilities.MeanReversion(yHeight);
            rowLength = MathUtilities.MeanReversion(rowLength);


            if (verifiedCellStart.x + columnLength < ColumnCount &&
                verifiedCellStart.x + columnLength >= 0 &&
                verifiedCellStart.z + rowLength < RowCount &&
                verifiedCellStart.z + rowLength >= 0 &&
                verifiedCellStart.y + yHeight < YAxisMax &&
                verifiedCellStart.y + yHeight >= 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region Debug Helpers

        public void DebugClickLogs()
        {
            CursorInformation clickInfo = CursorUtilities.CursorToWorldPosition(GridLayer);
            if (clickInfo.DidHit)
            {
                Debug.Log($"Hit Point: {clickInfo.Point}");

                Vector3Int cellCoords = GetCellCoordinatesFromWorldSpace(clickInfo.Point);
                Debug.Log(cellCoords);

                if(IsValidCellCoordinate(cellCoords))
                {
                    Debug.Log(GridCells[cellCoords].IsAvailable);
                }

                Debug.Log("Not a valid grid cell");
            }
        }

        /// <summary>
        /// Draws and prints debugging tools for grids if enabled.
        /// </summary>
        /// <remarks>
        /// Takes the 2D representation of the grid, meaning that it ignores the y-dimension.
        /// </remarks>
        public void DebugGridAs2D()
        {
            if (DrawDebug)
            {
                DebugDraw2DGridLines();
            }

            if (PrintDebug)
            {
                Debug2DCornerCoordinates();
            }
        }

        public void DebugDraw2DGridLines()
        {
            for (int row = 0; row < RowCount + 1; row++)
            {
                Debug.DrawRay(new Vector3(0, 0, row * CellSize) + BottomLeftCorner, new Vector3(ColumnCount * CellSize, 0, 0), Color.red, Mathf.Infinity);
            }

            for (int col = 0; col < ColumnCount + 1; col++)
            {
                Debug.DrawRay(new Vector3(col * CellSize, 0, 0) + BottomLeftCorner, new Vector3(0, 0, RowCount * CellSize), Color.green, Mathf.Infinity);
            }
        }

        public void Debug2DCornerCoordinates()
        {
            Vector3 center = Transform.position;
            Vector3 halfSize = new Vector3(ScaledBounds.x / 2, 0, ScaledBounds.z / 2);

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
    }
}
