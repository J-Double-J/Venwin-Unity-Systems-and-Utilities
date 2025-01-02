using UnityEngine;

namespace Venwin.Grid
{
    /// <summary>
    /// Configuration details for any raycasting projection onto the grid surface.
    /// </summary>
    public class ProjectionDetails
    {
        /// <summary>
        /// If true, it grabs all the objects on <see cref="GridSurfaceLayer"/> that the raycast goes through.
        /// </summary>
        /// <remarks>
        /// This allows cells to exist under one another if using multilayered grid structures.
        /// </remarks>
        public bool CastThroughAllObjects { get; private set; }

        /// <summary>
        /// Y-index that is above all elements on the grid. The raycast will start here.
        /// </summary>
        public float StartingYIndexAboveGrid { get; private set; }

        /// <summary>
        /// The lowest point to check to on the grid before halting the ray cast.
        /// </summary>
        public float LowestYIndex { get; private set; }

        /// <summary>
        /// The layer to search for the surface that the <see cref="GridCell"/>s reside on.
        /// </summary>
        public LayerMask GridSurfaceLayer { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="castThroughAllObjects">If true, it grabs all the objects on <see cref="GridSurfaceLayer"/> that the raycast goes through.</param>
        /// <param name="startingYIndexAboveGrid">Y-index that is above all elements on the grid. The raycast will start here.</param>
        /// <param name="lowestYIndex">The lowest point to check to on the grid before halting the ray cast.</param>
        /// <param name="gridSurfaceLayer">The layer to search for the surface that the <see cref="GridCell"/>s reside on.</param>
        public ProjectionDetails(bool castThroughAllObjects, float startingYIndexAboveGrid, float lowestYIndex, LayerMask gridSurfaceLayer)
        {
            CastThroughAllObjects = castThroughAllObjects;
            StartingYIndexAboveGrid = startingYIndexAboveGrid;
            LowestYIndex = lowestYIndex;
            GridSurfaceLayer = gridSurfaceLayer;
        }
    }
}
