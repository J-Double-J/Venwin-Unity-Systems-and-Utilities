using UnityEngine;
using Venwin.Systems;
using Venwin.Utilities;

namespace Venwin.Grid
{
    public class GhostGridObject : GhostGameObject
    {
        [Tooltip("This is the actual object that has information regarding the real grid object")]
        [SerializeField] protected GridObject realGridObject;
        [SerializeField] private LayerMask gridLayerMask;

        public OriginPosition OriginPosition { get; protected set; }

        /// <summary>
        /// Set this to true if you want to do a weaker placement check and are willing to "overwrite" the grid.
        /// </summary>
        [HideInInspector] public bool ForcingPlacement { get; set; } = false;

        protected override void Awake()
        {
            base.Awake();

            realGridObject = Instantiate(realGridObject);
            realGridObject.gameObject.SetActive(false);
        }

        public virtual void Rotate()
        {
            transform.Rotate(0f, 90f, 0f);
            OriginPosition = OriginPosition.RotateClockwise();
            realGridObject.RotateObject();
        }

        /// <summary>
        /// Follows the cursor with the ghost object. This implementation snaps the ghost object to the coordinates of any grid it finds.
        /// </summary>
        protected override void FollowCursor()
        {
            if(IsGhostingOnBadLayers()) { return; }

            (CursorInformation cI, IGridHolder gridHolder) = CursorUtilities.GameComponentAtCursorPosition<IGridHolder>(gridLayerMask);
            if (!cI.DidHit) { OnInvalidGhostOffLayer(); return; }

            GridCell cell = gridHolder.HeldGrid.GetCellFromWorldSpace(cI.Point);
            if(!IsValidPlacement(cell, gridHolder.HeldGrid))
            {
                OnInvalidGhostOnLayer();
                return;
            }

            transform.position = gridHolder.HeldGrid.GetWorldPositionFromCellAndRotation(cell.GridCoordinates, realGridObject);

            GhostIsValid = true;
            SetRenderersActive(true);
        }

        protected virtual bool IsValidPlacement(GridCell cell, Grid grid)
        {
            if (ForcingPlacement)
            {
                return grid.AreAllGridCellsPlaceable(cell.GridCoordinates, realGridObject.GetDimensions(), OriginPosition);
            }
            else
            {
                return grid.AreAllGridCellsAvailable(cell.GridCoordinates, realGridObject);
            }
        }
    }
}
