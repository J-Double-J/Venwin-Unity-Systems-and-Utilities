using UnityEngine;

#nullable enable

namespace Venwin.Grid
{
    public class GridObject : MonoBehaviour
    {
        #region Declarations

        [SerializeField] protected GridDimensions Dimensions;
        [SerializeField] protected GridObjectCellDetails cellDetails;

        #endregion Declarations

        #region Properties

        [field: SerializeField, HideInInspector]
        public OriginPosition ObjectOriginPosition { get; protected set; }

        public GridObjectCellDetails GridObjectCellDetails
        {
            get => cellDetails;
            protected set => cellDetails = value;
        }

        [HideInInspector]
        public GridCell? StartingCell { get; set; }

        #endregion Properties

        public virtual void Awake()
        {
            cellDetails ??= new GridObjectCellDetails();
            cellDetails.Initialize(this);
        }

        // Needed so that GridObject's can't be manipulated but still avaiable in serializer.
        /// <summary>
        /// Gets the grid dimensions of this grid object.
        /// </summary>
        /// <returns>The dimensions.</returns>
        public GridDimensions GetDimensions()
        {
            return Dimensions;
        }

        /// <summary>
        /// Called when a <see cref="GridObject"/> is placed on the grid.
        /// </summary>
        public virtual void OnPlaced()
        {
            cellDetails.OnPlaced();
        }

        public virtual void OnRemoved()
        {
            cellDetails.OnRemoved();
        }

        /// <summary>
        /// Rotates the <see cref="GridObject"/> clockwise 90 degrees.
        /// </summary>
        public virtual void RotateObject()
        {
            transform.Rotate(0f, 90f, 0f);

            ObjectOriginPosition = ObjectOriginPosition.RotateClockwise();
        }
    }
}
