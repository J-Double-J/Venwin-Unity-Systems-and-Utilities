using System.Collections.Generic;
using UnityEngine;

#nullable enable

namespace Venwin.Grid
{
    /// <summary>
    /// Non-generic implementation of a grid cell. Used when the type of object stored doesn't matter.
    /// </summary>
    public class GridCell
    {
        #region Declarations

        private HashSet<GridCell> neighbors = new();

        #endregion

        public virtual bool IsAvailable { get; set; } = true;

        public int CellSize { get; protected set; }

        /// <summary>
        /// Gets the grid coordinates of this cell.
        /// </summary>
        public Vector3Int GridCoordinates { get; protected set; }
        
        /// <summary>
        /// Gets the coordinates for this grid cell in world space.
        /// </summary>
        public Vector3 WorldSpaceCoordinates { get; protected set; }

        /// <summary>
        /// Gets or sets the CellDetails assigned to this cell.
        /// </summary>
        public CellDetails? CellDetails { get; set; } = null;

        public Grid OwningGrid { get; }

        #region Navigation Properties

        /// <summary>
        /// Gets or sets whether this cell can be navigated through.
        /// </summary>
        public virtual bool IsNavigatable { get; set; } = true;

        public virtual IReadOnlyCollection<GridCell> Neighbors { get { return neighbors; } }

        public int CostToEnter { get; set; } = 1;

        #endregion

        public GridCell(Grid owningGrid, int cellSize, Vector3Int coordinates, Vector3 worldSpaceCoordinates)
        {
            OwningGrid = owningGrid;
            CellSize = cellSize;
            GridCoordinates = coordinates;
            WorldSpaceCoordinates = worldSpaceCoordinates;
        }

        public Vector3 CenterOfCellWorldSpace()
        {
            return new Vector3((float)(WorldSpaceCoordinates.x + CellSize / 2.0), 0, (float)(WorldSpaceCoordinates.z + CellSize / 2.0));
        }

        #region Navigation

        /// <summary>
        /// Tries to add a neighbor cell.
        /// </summary>
        /// <param name="cell">Cell that is the neighbor</param>
        /// <returns>True if successfully added as a neighbor, false if this cell is already aware of this neighbor.</returns>
        public virtual bool AddNeighbor(GridCell cell)
        {
            return neighbors.Add(cell);
        }

        /// <summary>
        /// Establishes a two way neighbor connection between grid cells.
        /// </summary>
        /// <param name="neighboringCell"></param>
        /// <returns>True if this cell could add the <paramref name="neighboringCell"/> to its <see cref="Neighbors"/>.</returns>
        public virtual bool AddConnectionBetweenCells(GridCell neighboringCell)
        {
            neighboringCell.AddNeighbor(this);
            return AddNeighbor(neighboringCell);
        }

        /// <summary>
        /// Removes the neighbor connection between two cells.
        /// </summary>
        /// <param name="neighboringCell">The neighboring cell to remove the connection to.</param>
        /// <returns>True if the <paramref name="neighboringCell"/> was removed from this cell's <see cref="Neighbors"/>.</returns>
        public virtual bool RemoveConnectionBetweenCells(GridCell neighboringCell)
        {
            neighboringCell.RemoveNeighborConnection(this);
            return neighbors.Remove(neighboringCell);
        }

        private void RemoveNeighborConnection(GridCell gridCell)
        {
            neighbors.Remove(gridCell);
        }

        public virtual void SetNeighbors(HashSet<GridCell> neighbors)
        {
            this.neighbors = neighbors;
        }

        /// <summary>
        /// Gets the cost to enter to enter this cell.
        /// </summary>
        /// <param name="leavingCell">The cell being moved from to enter this one.</param>
        /// <returns>The cost to enter this cell.</returns>
        public virtual int GetCostToEnter(GridCell leavingCell)
        {
            return CostToEnter;
        }

        #endregion
    }
}
