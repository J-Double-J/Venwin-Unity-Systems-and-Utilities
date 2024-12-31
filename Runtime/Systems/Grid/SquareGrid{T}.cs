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
                        Vector3 worldSpaceCoordinates = new(col * CellSize + BottomLeftCorner.x,
                                                            yAxis * CellSize + BottomLeftCorner.y,
                                                            row * CellSize + BottomLeftCorner.z);

                        Vector3Int gridCoord = new(col, yAxis, row);

                        if (CellCreationCallback == null)
                        {
                            GridCells[gridCoord] = new GridCell<T>(this, CellSize, gridCoord, worldSpaceCoordinates);
                        }
                        else
                        {
                            GridCells[gridCoord] = CellCreationCallback(this, CellSize, gridCoord, worldSpaceCoordinates);
                        }
                    }
                }
            }
        }
    }
}
