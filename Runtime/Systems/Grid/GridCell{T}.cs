using UnityEngine;

#nullable enable

namespace Venwin.Grid
{
    /// <summary>
    /// Represents a cell on the grid that takes <typeparamref name="T"/> grid objects.
    /// </summary>
    /// <typeparam name="T">The type of grid objects accepted on this grid cell.</typeparam>
    public class GridCell<T> : GridCell where T : GridObject
    {
        public T? CurrentObject { get; private set; } = null;

        public GridCell(Grid owningGrid, int cellSize, Vector3Int coordinates, Vector3 worldSpaceCoordinates)
            : base(owningGrid, cellSize, coordinates, worldSpaceCoordinates)
        {
        }

        /// <summary>
        /// Adds the <paramref name="givenObject"/> without availability checks.
        /// </summary>
        /// <param name="givenObject">The object to assign to this cell.</param>
        public void AddObject(T givenObject)
        {
            SetGameObject(givenObject);
        }

        /// <summary>
        /// Tries to add a game object to the <see cref="GridCell"/> by doing an availability check.
        /// </summary>
        /// <param name="givenObject">Object that is attempted being added.</param>
        /// <returns>True if the cell could take the object, else false.</returns>
        public virtual bool TryAddObject(T givenObject)
        {
            if(!IsAvailable)
            {
                return false;
            }

            SetGameObject(givenObject);
            return true;
        }

        /// <summary>
        /// Sets the game object that is on this cell, ignoring availability checks.
        /// </summary>
        /// <param name="gameObject">Given game object that is now on this cell.</param>
        protected virtual void SetGameObject(T gameObject)
        {
            CurrentObject = gameObject;
            IsAvailable = false;
        }

        /// <summary>
        /// Remove the currently assigned object on this <see cref="GridCell"/>.
        /// </summary>
        public virtual void RemoveCurrentObject()
        {
            CurrentObject = null;
            IsAvailable = true;
        }
    }
}
