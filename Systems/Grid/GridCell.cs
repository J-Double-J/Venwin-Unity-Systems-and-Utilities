using UnityEngine;

#nullable enable

namespace Venwin.Grid
{
    public class GridCell<T> where T : GridObject
    {
        public T? CurrentObject { get; private set; } = null;
        public virtual bool IsAvailable { get; set; } = true;
        public int CellSize { get; private set; }
        public Vector3Int Coordinates { get; private set; }
        public Vector3 WorldSpaceCoordinates {get; private set; }

        /// <summary>
        /// Gets or sets the CellDetails assigned to this cell.
        /// </summary>
        public CellDetails? CellDetails { get; set; } = null;

        public GridCell(int cellSize, Vector3Int coordinates, Vector3 worldSpaceCoordinates)
        {
            CellSize = cellSize;
            Coordinates = coordinates;
            WorldSpaceCoordinates = worldSpaceCoordinates;
        }

        public void AddObject(T givenObject)
        {
            SetGameObject(givenObject);
        }

        public virtual bool TryAddObject(T givenObject)
        {
            if(!IsAvailable)
            {
                return false;
            }

            SetGameObject(givenObject);
            return true;
        }

        protected virtual void SetGameObject(T gameObject)
        {
            CurrentObject = gameObject;
            IsAvailable = false;
        }

        public Vector3 CenterOfCellWorldSpace()
        {
            return new Vector3((float)(WorldSpaceCoordinates.x + CellSize / 2.0), 0, (float)(WorldSpaceCoordinates.z + CellSize / 2.0));
        }
    }
}
