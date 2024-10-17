using System;
using System.Collections.Generic;
using UnityEngine;

#nullable enable

namespace Venwin.Grid
{
    public class Grid<T> : Grid where T : GridObject
    {
        /// <summary>
        /// A casted version of <see cref="GridCells"/>.
        /// </summary>
        public GridCell<T>[,] GenericGridCells { get; protected set; }

        // Runtime fields
        //public GridCell<T>[,] GridCells { get; private set; }

        public Grid(Transform transform, Mesh mesh, int cellSize, LayerMask gridLayer)
            : base(transform, mesh, cellSize, gridLayer)
        {
            DebugDrawGridLines();
            DebugCornerCoordinates();

            GenericGridCells = (GridCell<T>[,])GridCells;
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
        /// Gets a <see cref="GridCell{T}"/> from the grid from a world space point.
        /// </summary>
        /// <param name="worldSpacePoint">Point in world space that is on the grid that can be mapped to the grid coordinate.</param>
        /// <returns>The <see cref="GridCell{T}"/> in that worldspace, else null if the <paramref name="worldSpacePoint"/> is not on the grid.</returns>
        public new GridCell<T>? GetCellFromWorldSpace(Vector3 worldSpacePoint)
        {
            Vector3Int cellCoordinates = GetCellCoordinatesFromWorldSpace(worldSpacePoint);

            if(cellCoordinates == InvalidCell) { return null; }

            return GenericGridCells[cellCoordinates.x, cellCoordinates.z];
        }

        /// <summary>
        /// Gets a <see cref="GridCell{T}"/> from the grid from a grid coordinate.
        /// </summary>
        /// <param name="gridCoordinate">A coordinate that is on the grid.</param>
        /// <returns>A grid cell if the coordinate was valid.</returns>
        public new GridCell<T>? GetCellFromGridCoordiantes(Vector3Int gridCoordinate)
        {
            if (!IsValidCellCoordinate(gridCoordinate))
            {
                return null;
            }

            return GenericGridCells[gridCoordinate.x, gridCoordinate.z];
        }

        /// <summary>
        /// Gets all the objects in surrounding grids of a given cell coordinate.
        /// </summary>
        /// <param name="cellCoordinate">Coordinate of the cell to search around.</param>
        /// <returns>A list of nullable grid elements starting from the north and going clockwise.</returns>
        public List<T?> GetAllGridObjectsOnGridAroundCell(Vector3Int cellCoordinate)
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
                gridElements.Add(GenericGridCells[cellCoordinate.z + 1, cellCoordinate.x].CurrentObject);
            } else { gridElements.Add(null); }

            // East
            if (IsValidCellCoordinate(cellCoordinate + new Vector3Int(1, 0, 0)))
            {
                gridElements.Add(GenericGridCells[cellCoordinate.z, cellCoordinate.x + 1].CurrentObject);
            } else { gridElements.Add(null); }

            // South
            if (IsValidCellCoordinate(cellCoordinate + new Vector3Int(0, 0, 1)))
            {
                gridElements.Add(GenericGridCells[cellCoordinate.z - 1, cellCoordinate.x].CurrentObject);
            } else { gridElements.Add(null); }

            // West
            if (IsValidCellCoordinate(cellCoordinate + new Vector3Int(0, 0, 1)))
            {
                gridElements.Add(GenericGridCells[cellCoordinate.z, cellCoordinate.x - 1].CurrentObject);
            } else { gridElements.Add(null); }

            return gridElements;
        }

        /// <summary>
        /// Tries to place the object on the grid. If its doesn't fit on the grid its not placed.
        /// </summary>
        /// <param name="gridObject">The grid object prefab.</param>
        /// <param name="startingCellCoordinate">The grid coordinate to spawn the object from.</param>
        /// <param name="instantiatedGridObject">The instantiated grid object in the game.</param>
        /// <param name="parent">The parent to assign this grid object to.</param>
        /// <returns>True if successfully placed.</returns>
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

            instantiatedGridObject = UnityEngine.Object.Instantiate(gridObject,
                                                                    GetWorldPositionFromCellAndRotation(startingCellCoordinate, gridObject),
                                                                    gridObject.ObjectOriginPosition.GetQuaternionBasedOnOriginPosition());
            if (parent != null)
            {
                instantiatedGridObject.transform.parent = parent.transform;
            }

            instantiatedGridObject.StartingCell = GetCellFromGridCoordiantes(startingCellCoordinate);

            Action<GridCell<T>, T, Vector3Int> addAction = (GridCell<T> gridCell, T gridObject1, Vector3Int _) => { gridCell.AddObject(gridObject1); };
            ExecuteActionOnGridCellsBasedOnObjectAndOrientation(addAction, instantiatedGridObject, startingCellCoordinate);

            instantiatedGridObject.GridObjectCellDetails.AssignGridDetailsToCells(startingCellCoordinate, this);

            ObjectPlacedOnGrid(instantiatedGridObject);

            return true;
        }

        /// <summary>
        /// Tries to remove a game object from the grid.
        /// </summary>
        /// <remarks>
        /// Because <see cref="Grid"/> is not a <see cref="MonoBehaviour"/> class it cannot and does not destroy any game objects that are removed from the grid.
        /// That must be handled outside this method call if needed.
        /// <br/>
        /// Order of operations: <see cref="GridObject.OnRemoved()"/> -> <see cref="GridObjectCellDetails.UnassignGridDetailsToCells{T}(Vector3Int, Grid{T})"/> -> <see cref="GridCell{T}.RemoveCurrentObject()"/>.
        /// <br/>This is the instantiation order reversed. 
        /// </remarks>
        /// <param name="targetGameObject"><typeparamref name="T"/> attempted to be removed.</param>
        /// <param name="startingCellCoordinate">The starting cell coordinate.</param>
        /// <returns>True if the object was successfully removed from the grid, else false.</returns>
        public bool TryRemoveObject(T targetGameObject)
        {
            if(targetGameObject == null)
            {
                Debug.LogError("Cannot remove a null grid object from the grid.");
                return false;
            }

            if (targetGameObject.StartingCell == null)
            {
                Debug.LogError($"The given game object: {targetGameObject}, is not considered to have a starting cell, and likely is incorrectly not put on the grid. Not removing.");
                return false;
            }

            try
            {
                bool gameObjectIsOccupyingAllCells = true;

                Vector3Int startingCellCoordinate = targetGameObject.StartingCell.GridCoordinates;

                Action<GridCell<T>, T, Vector3Int> verifyObjectIsOccupyingAllCells = (gridCell, gridObject, _) => {
                    if (gridCell.CurrentObject != gridObject) { gameObjectIsOccupyingAllCells = false; }
                };
                ExecuteActionOnGridCellsBasedOnObjectAndOrientation(verifyObjectIsOccupyingAllCells, targetGameObject, startingCellCoordinate);

                if (!gameObjectIsOccupyingAllCells)
                {
                    Debug.LogError("The given object is not considered to be on all the cells that the game thinks it should be occupying. Not removing due to ambiguity.");
                    return false;
                }

                targetGameObject.OnRemoved();

                targetGameObject.GridObjectCellDetails.UnassignGridDetailsToCells(startingCellCoordinate, this);

                Action<GridCell<T>, T, Vector3Int> removeObjectFromGrid = (gridCell, gridObject, _) => { gridCell.RemoveCurrentObject(); };
                ExecuteActionOnGridCellsBasedOnObjectAndOrientation(removeObjectFromGrid, targetGameObject, startingCellCoordinate);
                
                targetGameObject.StartingCell = null;
            }
            catch (Exception e)
            {
                Debug.LogError($"Could not remove object due to {e.Message}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Executes an action based on the grid object and passes the correct grid coordinates based on the <see cref="GridObject"/>'s orientation.
        /// </summary>
        ///
        /// <remarks>
        /// This is the generic version of <see cref="Grid.ExecuteActionOnGridCellsBasedOnObjectAndOrientation(Action{GridCell, GridObject, Vector3Int}, GridObject, Vector3Int)"/>.<br/>
        /// <paramref name="action"/> will be executed on every cell on the grid based on the dimensions of the <see cref="GridObject"/>.<br/>It takes the following:
        ///<list type = "bullet" >
        /// <item><see cref="GridCell{T}"/> - Current <see cref="GridCell"/> that the <paramref name="action"/> will be executed on.</item>
        /// <item><typeparamref name="T"/> - The <see cref="GridObject"/> that may be relevant for the cell.</item>
        /// <item><see cref="Vector3Int"/> - The current local cell coordinate of the <see cref="GridObject"/>. (Not affected by rotation)</item>
        /// </list>
        /// <br/>
        /// This does mean that <see cref="GridCell"/> and <see cref="Vector3Int"/> can point to different things. <see cref="GridCell"/> is for if you want the Grid/World Space,
        /// whereas the local coordinate is better for knowing which cell on a <see cref="GridObject"/> is being assigned to that <see cref="GridCell"/>.
        /// </remarks>
        /// 
        /// <param name="action">
        /// <list type="bullet">
        /// <item><see cref="GridCell{T}"/>Current cell that the action will be executed on.</item>
        /// <item><see cref="GridObject"/>The grid object that may be relevant for the cell. Uses <paramref name="gridObject"/>.</item>
        /// <item>The local cell coordinate of the <see cref="GridObject"/>. (Not affected by rotation)</item>
        /// </list>
        /// </param>
        /// <param name="gridObject">The <see cref="GridObject"/> that is relevant for orientation and dimensions.</param>
        /// <param name="startingCell">The starting cell for the operation.</param>
        /// <exception cref="Exception">Can throw any exception that could occur as a result of passed in <paramref name="action"/>.</exception>
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
                                GridCell<T> cell = GenericGridCells[startingCell.x + col, startingCell.z + row];

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
                                GridCell<T> cell = GenericGridCells[startingCell.x + row, startingCell.z - col];

                                action(cell, gridObject, new Vector3Int(col, 0, row));
                            }
                        }
                        break;

                    case OriginPosition.TopRight:
                        for (int col = 0; col < dimensions.X; col++)
                        {
                            for (int row = 0; row < dimensions.Z; row++)
                            {
                                GridCell<T> cell = GenericGridCells[startingCell.x - col, startingCell.z - row];

                                action(cell, gridObject, new Vector3Int(col, 0, row));
                            }
                        }
                        break;

                    case OriginPosition.BottomRight:
                        for (int col = 0; col < dimensions.X; col++)
                        {
                            for (int row = 0; row < dimensions.Z; row++)
                            {
                                GridCell<T> cell = GenericGridCells[startingCell.x - row, startingCell.z + col];

                                action(cell, gridObject, new Vector3Int(col, 0, row));
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"An error occured when trying to execute an anonymous action on the grid based on grid object and orientation: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Creates the grid cells
        /// </summary>
        protected override void CreateGridCells()
        {
            GridCells = new GridCell<T>[ColumnCount, RowCount];

            for (int col = 0; col < ColumnCount; col++)
            {
                for (int row = 0; row < RowCount; row++)
                {
                    Vector3 worldSpaceCoordinates = new(col * CellSize + BottomLeftCorner.x, 0, row * CellSize + BottomLeftCorner.z);
                    GridCells[col, row] = new GridCell<T>(this, CellSize, new Vector3Int(col, 0, row), worldSpaceCoordinates);
                }
            }
        }

        protected override void ConfigureGridCellsForNavigation()
        {
            for (int col = 0; col < ColumnCount; col++)
            {
                for (int row = 0; row < RowCount; row++)
                {
                    if (GridIsNavigatable)
                    {
                        AssignCellNeighbors(GridCells[col, row]);
                    }
                    
                    GridCells[col, row].IsNavigatable = GridIsNavigatable;
                }
            }
        }

        /// <summary>
        /// Finds the <paramref name="currentCell"/>'s neighbors and makes it aware of them.
        /// </summary>
        /// <param name="currentCell">The cell getting neighbors</param>
        protected virtual void AssignCellNeighbors(GridCell currentCell)
        {
            if (IsValidCellCoordinate(new Vector3Int(currentCell.GridCoordinates.x, 0, currentCell.GridCoordinates.z + 1)))
            {
                currentCell.AddNeighbor(GridCells[currentCell.GridCoordinates.x, currentCell.GridCoordinates.z + 1]);
            }

            if (IsValidCellCoordinate(new Vector3Int(currentCell.GridCoordinates.x + 1, 0, currentCell.GridCoordinates.z)))
            {
                currentCell.AddNeighbor(GridCells[currentCell.GridCoordinates.x + 1, currentCell.GridCoordinates.z]);
            }

            if (IsValidCellCoordinate(new Vector3Int(currentCell.GridCoordinates.x, 0, currentCell.GridCoordinates.z - 1)))
            {
                currentCell.AddNeighbor(GridCells[currentCell.GridCoordinates.x, currentCell.GridCoordinates.z - 1]);
            }

            if (IsValidCellCoordinate(new Vector3Int(currentCell.GridCoordinates.x - 1, 0, currentCell.GridCoordinates.z)))
            {
                currentCell.AddNeighbor(GridCells[currentCell.GridCoordinates.x - 1, currentCell.GridCoordinates.z + 0]);
            }
        }
    }
}
