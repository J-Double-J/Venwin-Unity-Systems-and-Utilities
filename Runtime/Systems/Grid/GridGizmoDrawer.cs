using UnityEngine;

namespace Venwin.Grid
{
    public class GridGizmoDrawer : MonoBehaviour
    {
        [SerializeField]
        public bool DrawDebug = true;

        public Grid Grid { get; set; }

        /// <summary>
        /// Draws the Physics.Overlap box used in <see cref="GridCell.CheckForUniqueRootObjectsWithinCellBounds(LayerMask)"/> for detection.
        /// </summary>
        public void DrawCellRayCastDetection()
        {
            if (Grid == null)
            {
                Debug.LogError($"{nameof(GridGizmoDrawer)} is not assigned a grid and cannot draw anything.");
                return;
            }

            if(!DrawDebug) { return; }

            foreach(GridCell cell in Grid.GridCells.Values)
            {
                Vector3 halfExtents = new(cell.CellSize, cell.CellSize, cell.CellSize);

                Gizmos.color = Color.blue;

                // Gizmos expects the full size of the box. The cell uses Physics.OverlapBox which expects half extents. So this should always be double
                // what appears in the Cell's raycast.
                Gizmos.DrawWireCube(cell.CenterOfCellWorldSpace + new Vector3(0, cell.CellSize / 2f, 0), new(cell.CellSize, cell.CellSize, cell.CellSize));
            }
        }
    }
}
