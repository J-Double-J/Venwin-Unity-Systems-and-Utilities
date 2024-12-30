using System;
using UnityEngine;

namespace Venwin.Grid
{
    /// <summary>
    /// This is a class that configures a simple grid of squares that can hold <typeparamref name="T"/>.
    /// </summary>
    /// <remarks>
    /// Base class assumes square grid cells at the moment, but this will likely change.<br/>
    /// Implementors should use the Square Grid still to prevent breaking code changes in future.
    /// </remarks>
    public class SquareGrid<GridCellT, T> : Grid<GridCellT, T> where GridCellT : GridCell<T> where T : GridObject
    {
        public SquareGrid(Transform transform, Mesh mesh, int cellSize, int yAxisMax, LayerMask gridLayer)
            : base(transform, mesh, cellSize, yAxisMax, gridLayer)
        {
        }

        public SquareGrid(Transform transform, Mesh mesh, int cellSize, int yAxisMax, LayerMask gridLayer, Func<Grid, int, Vector3Int, Vector3, GridCellT> cellCreationCallback)
            : base(transform, mesh, cellSize, yAxisMax, gridLayer, cellCreationCallback)
        {
        }

        /// <inheritdoc/>
        protected override void ConfigureGridCellsForNavigation()
        {
            for (int col = 0; col < ColumnCount; col++)
            {
                for(int yAxis = 0; yAxis < YAxisMax; yAxis++)
                {
                    for (int row = 0; row < RowCount; row++)
                    {
                        if (GridIsNavigatable)
                        {
                            AssignCellNeighbors(GridCells[col, yAxis, row]);
                        }

                        GridCells[col, yAxis, row].IsNavigatable = GridIsNavigatable;
                    }
                }
            }
        }

        /// <summary>
        /// Finds the <paramref name="currentCell"/>'s neighbors and makes it aware of them.
        /// </summary>
        /// <param name="currentCell">The cell getting neighbors</param>
        protected override void AssignCellNeighbors(GridCell currentCell)
        {
            if (IsValidCellCoordinate(new Vector3Int(currentCell.GridCoordinates.x, currentCell.GridCoordinates.y, currentCell.GridCoordinates.z + 1)))
            {
                currentCell.AddNeighbor(GridCells[currentCell.GridCoordinates.x, currentCell.GridCoordinates.y, currentCell.GridCoordinates.z + 1]);
            }

            if (IsValidCellCoordinate(new Vector3Int(currentCell.GridCoordinates.x + 1, currentCell.GridCoordinates.y, currentCell.GridCoordinates.z)))
            {
                currentCell.AddNeighbor(GridCells[currentCell.GridCoordinates.x + 1, currentCell.GridCoordinates.y, currentCell.GridCoordinates.z]);
            }

            if (IsValidCellCoordinate(new Vector3Int(currentCell.GridCoordinates.x, currentCell.GridCoordinates.y, currentCell.GridCoordinates.z - 1)))
            {
                currentCell.AddNeighbor(GridCells[currentCell.GridCoordinates.x, currentCell.GridCoordinates.y, currentCell.GridCoordinates.z - 1]);
            }

            if (IsValidCellCoordinate(new Vector3Int(currentCell.GridCoordinates.x - 1, currentCell.GridCoordinates.y, currentCell.GridCoordinates.z)))
            {
                currentCell.AddNeighbor(GridCells[currentCell.GridCoordinates.x - 1, currentCell.GridCoordinates.y, currentCell.GridCoordinates.z + 0]);
            }
        }

        /// <inheritdoc/>
        protected override void CreateGridCells()
        {
            GridCells = new GridCell[ColumnCount, YAxisMax, RowCount];

            for (int col = 0; col < ColumnCount; col++)
            {
                for(int yAxis = 0; yAxis < YAxisMax; yAxis++)
                {
                    for (int row = 0; row < RowCount; row++)
                    {
                        Vector3 worldSpaceCoordinates = new(col * CellSize + BottomLeftCorner.x,
                                                            yAxis * CellSize + BottomLeftCorner.y,
                                                            row * CellSize + BottomLeftCorner.z);
                        if (CellCreationCallback == null)
                        {
                            GridCells[col, yAxis, row] = new GridCell<T>(this, CellSize, new Vector3Int(col, yAxis, row), worldSpaceCoordinates);
                        }
                        else
                        {
                            GridCells[col, yAxis, row] = CellCreationCallback(this, CellSize, new Vector3Int(col, yAxis, row), worldSpaceCoordinates);
                        }
                    }
                }
            }
        }
    }
}
