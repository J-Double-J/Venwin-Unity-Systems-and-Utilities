using NUnit.Framework;
using System;
using UnityEngine;

namespace Venwin.Grid
{
    public class GridObject : MonoBehaviour
    {
        [SerializeField] protected GridDimensions Dimensions;
        [SerializeField] protected GridObjectCellDetails cellDetails;

        public OriginPosition ObjectOriginPosition { get; protected set; }

        public GridObjectCellDetails GridObjectCellDetails
        {
            get => cellDetails;
            protected set => cellDetails = value;
        }

        public void Awake()
        {
            cellDetails = new GridObjectCellDetails(this);
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
        /// <param name="globalGridPosition">The position that the grid was instantiated at (ie the origin point's cell position)</param>
        public virtual void OnPlaced(Vector3 globalGridPosition)
        {

        }

        public Quaternion GetQuaternionBasedOnOriginPosition()
        {
            return ObjectOriginPosition switch
            {
                OriginPosition.BottomLeft => Quaternion.identity,
                OriginPosition.TopLeft => Quaternion.Euler(0, 90f, 0),
                OriginPosition.TopRight => Quaternion.Euler(0, 180f, 0),
                OriginPosition.BottomRight => Quaternion.Euler(0, 270f, 0),
                _ => Quaternion.identity,
            };
        }

        public virtual void RotateObject()
        {
            transform.Rotate(0f, 90f, 0f);

            ObjectOriginPosition = ObjectOriginPosition switch
            {
                OriginPosition.BottomLeft => OriginPosition.TopLeft,
                OriginPosition.TopLeft => OriginPosition.TopRight,
                OriginPosition.TopRight => OriginPosition.BottomRight,
                OriginPosition.BottomRight => OriginPosition.BottomLeft,
                _ => throw new NotImplementedException($"Origin position could not be rotated due to unknown state: {ObjectOriginPosition}"),
            };
        }
    }
}
