using System.Collections.Generic;
using UnityEngine;
using Venwin.Utilities;

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
        /// Gets the center of the Grid Cell in World Space.
        /// </summary>
        public Vector3 CenterOfCellWorldSpace { get; protected set; }

        /// <summary>
        /// Gets or sets the CellDetails assigned to this cell.
        /// </summary>
        public CellDetails? CellDetails { get; set; } = null;

        /// <summary>
        /// Gets or sets details about any ramp that might be in this cell.
        /// </summary>
        public GridCellRampDetails? CellRampDetails { get; set; }

        /// <summary>
        /// Gets the grid that this cell is on.
        /// </summary>
        public Grid OwningGrid { get; }

        public bool HasLayerTransitionDetails { get; private set; } = false;


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
            CenterOfCellWorldSpace = new Vector3((float)(WorldSpaceCoordinates.x + CellSize / 2.0), WorldSpaceCoordinates.y, (float)(WorldSpaceCoordinates.z + CellSize / 2.0));
        }

        /// <summary>
        /// A grid cell that is marked as unaccessible is one that is blocked and cannot be normally accessed.
        /// </summary>
        /// <example>
        /// A cell sitting directly between two game objects.
        /// </example>
        protected virtual void MarkCellAsUnaccessible()
        {
            IsNavigatable = false;
            IsAvailable = false;
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

        #region Cell Detection

        /// <summary>
        /// Grabs all root GameObjects found within a cell in a given layer mask. 
        /// </summary>
        /// <remarks>
        /// Ignores trigger colliders.
        /// </remarks>
        /// <param name="layerMask">Layer Mask to search on.</param>
        /// <returns>A hashset of unique root game objects.</returns>
        public HashSet<GameObject> CheckForUniqueRootObjectsWithinCellBounds(LayerMask layerMask)
        {
            // We want to detect only above the grid cell, so we add to the y-axis of the center so that the bottom of the drawn box is on top of the cell.
            Collider[] hitColliders = Physics.OverlapBox(CenterOfCellWorldSpace + new Vector3(0, CellSize / 2f, 0),
                                                         new Vector3(CellSize / 2f, CellSize / 2f, CellSize / 2f),
                                                         Quaternion.identity,
                                                         layerMask,
                                                         QueryTriggerInteraction.Ignore);

            HashSet<GameObject> uniqueObjects = new();

            foreach(Collider collider in hitColliders)
            {
                GameObject rootObject = collider.transform.root.gameObject;
                uniqueObjects.Add(rootObject);
            }

            return uniqueObjects;
        }

        /// <summary>
        /// Checks for GameObjects in its cell and returns all root game objects in the layer that have a specified component.
        /// </summary>
        /// <remarks>
        /// Ignores trigger colliders.<br/>
        /// If one root game object contains multiple of the same component, it will <strong>only</strong> return the first one.
        /// </remarks>
        /// <typeparam name="T">Type of component to search for.</typeparam>
        /// <param name="layerMask">Layer Mask to look within.</param>
        /// <returns>A hashset of components that each have a unique root game object.</returns>
        public HashSet<T> CheckForUniqueRootObjectsWithinCellBounds<T>(LayerMask layerMask) where T : Component
        {
            // We want to detect only above the grid cell, so we add to the y-axis of the center so that the bottom of the drawn box is on top of the cell.
            Collider[] hitColliders = Physics.OverlapBox(CenterOfCellWorldSpace + new Vector3(0, CellSize / 2f, 0),
                                                         new Vector3(CellSize / 2f, CellSize / 2f, CellSize / 2f),
                                                         Quaternion.identity,
                                                         layerMask,
                                                         QueryTriggerInteraction.Ignore);

            HashSet<T> uniqueObjects = new();

            foreach (Collider collider in hitColliders)
            {
                GameObject rootObject = collider.transform.root.gameObject;

                T? searchedForObject = ParentChildUtilities.GetComponentInParentOrChildren<T>(rootObject);
                
                if(searchedForObject != null)
                {
                    uniqueObjects.Add(searchedForObject);
                }
            }

            return uniqueObjects;
        }

        /// <summary>
        /// Checks for GameObjects in its cell and returns all game objects in the layer that have a specified component.
        /// </summary>
        /// <remarks>
        /// Ignores trigger colliders.<br/>
        /// Will <strong>only</strong> check the game object any <see cref="Collider"/> is attached is to.
        /// </remarks>
        /// <typeparam name="T">Type of component to search for.</typeparam>
        /// <param name="layerMask">Layer Mask to look within.</param>
        /// <returns>A hashset of game objects that each of the search for component./returns>
        public HashSet<T> CheckForUniqueGameObjectsWithinCellBounds<T>(LayerMask layerMask) where T : Component
        {
            // We want to detect only above the grid cell, so we add to the y-axis of the center so that the bottom of the drawn box is on top of the cell.
            Collider[] hitColliders = Physics.OverlapBox(CenterOfCellWorldSpace + new Vector3(0, CellSize / 2f, 0),
                                                         new Vector3(CellSize / 2f, CellSize / 2f, CellSize / 2f),
                                                         Quaternion.identity,
                                                         layerMask,
                                                         QueryTriggerInteraction.Ignore);

            HashSet<T> uniqueObjects = new();

            foreach (Collider collider in hitColliders)
            {
                GameObject gameObject = collider.gameObject;

                if (gameObject.TryGetComponent(out T searchedForObject))
                {
                    uniqueObjects.Add(searchedForObject);
                }
            }

            return uniqueObjects;
        }

        #endregion
    }
}
