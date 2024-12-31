using UnityEngine;

namespace Venwin.Grid
{
    /// <summary>
    /// This is a class that configures a simple grid of squares.
    /// </summary>
    /// <remarks>
    /// Base class assumes square grid cells at the moment, but this will likely change.<br/>
    /// Implementors should use the Square Grid still to prevent breaking code changes in future.
    /// </remarks>
    public class SquareGrid : Grid
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="mesh"></param>
        /// <param name="cellSize"></param>
        /// <param name="gridLayer"></param>
        public SquareGrid(Transform transform, Mesh mesh, int cellSize, int yAxisMax, LayerMask gridLayer)
            : base(transform, mesh, cellSize, yAxisMax, gridLayer)
        {
        }

        /// <inheritdoc/>
        protected override void ConfigureGridCellsForNavigation()
        {
            for (int col = 0; col < ColumnCount; col++)
            {
                for (int yAxis = 0; yAxis < YAxisMax; yAxis++)
                {
                    for (int row = 0; row < RowCount; row++)
                    {
                        Vector3Int gridCoord = new Vector3Int(col, yAxis, row);

                        if (GridIsNavigatable)
                        {
                            AssignCellNeighbors(GridCells[gridCoord]);
                        }

                        GridCells[gridCoord].IsNavigatable = GridIsNavigatable;
                    }
                }
            }
        }

        /// <summary>
        /// Finds the <paramref name="currentCell"/>'s neighbors and makes it aware of them.
        /// </summary>
        /// <param name="currentCell">The cell getting neighbors</param>
        protected virtual void AssignCellNeighbors(GridCell currentCell)
        {
            if (IsValidCellCoordinate(new Vector3Int(currentCell.GridCoordinates.x, currentCell.GridCoordinates.y, currentCell.GridCoordinates.z + 1)))
            {
                currentCell.AddNeighbor(GridCells[new Vector3Int(currentCell.GridCoordinates.x, currentCell.GridCoordinates.y, currentCell.GridCoordinates.z + 1)]);
            }

            if (IsValidCellCoordinate(new Vector3Int(currentCell.GridCoordinates.x + 1, currentCell.GridCoordinates.y, currentCell.GridCoordinates.z)))
            {
                currentCell.AddNeighbor(GridCells[new Vector3Int(currentCell.GridCoordinates.x + 1, currentCell.GridCoordinates.y, currentCell.GridCoordinates.z)]);
            }

            if (IsValidCellCoordinate(new Vector3Int(currentCell.GridCoordinates.x, currentCell.GridCoordinates.y, currentCell.GridCoordinates.z - 1)))
            {
                currentCell.AddNeighbor(GridCells[new Vector3Int(currentCell.GridCoordinates.x, currentCell.GridCoordinates.y, currentCell.GridCoordinates.z - 1)]);
            }

            if (IsValidCellCoordinate(new Vector3Int(currentCell.GridCoordinates.x - 1, currentCell.GridCoordinates.y, currentCell.GridCoordinates.z)))
            {
                currentCell.AddNeighbor(GridCells[new Vector3Int(currentCell.GridCoordinates.x - 1, currentCell.GridCoordinates.y, currentCell.GridCoordinates.z)]);
            }
        }

        /// <inheritdoc/>
        protected override void CreateGridCells()
        {
            GridCells = new();

            for (int col = 0; col < ColumnCount; col++)
            {
                for(int yAxis = 0; yAxis < YAxisMax; yAxis++)
                {
                    for (int row = 0; row < RowCount; row++)
                    {
                        Vector3Int gridCoord = new Vector3Int(col, yAxis, row);
                        Vector3 worldSpaceCoordinates = new(col * CellSize + BottomLeftCorner.x, yAxis * CellSize + BottomLeftCorner.y, row * CellSize + BottomLeftCorner.z);
                        GridCells[gridCoord] = new GridCell(this, CellSize, gridCoord, worldSpaceCoordinates);
                    }
                }
            }
        }
    }
}
