#nullable enable

namespace Venwin.Grid
{
    /// <summary>
    /// Interface denoting that a given object is a detail.
    /// </summary>
    public interface IGridDetail
    {
        /// <summary>
        /// Executed when the <see cref="GridObject"/> that owns this <see cref="IGridDetail"/> is placed on the grid.
        /// </summary>
        /// <param name="gridObject">Owning grid object.</param>
        public void OnOwningGridObjectPlaced(GridObject gridObject);

        /// <summary>
        /// Executed during Update frame calls.
        /// </summary>
        public void OnOwningGridObjectUpdate();

        /// <summary>
        /// Executed when the owning <see cref="GridObject"/> is removed from the grid.
        /// </summary>
        /// <param name="gridObject">Owning grid object</param>
        public void OnOwningGridObjectRemoved(GridObject gridObject);
    }

    /// <summary>
    /// More specific <see cref="IGridDetail"/> that specifies that the attached object is a monobehavior.
    /// </summary>
    public interface IGridDetailMonobehavior : IGridDetail
    {
        /// <summary>
        /// Gets or sets the <see cref="GridCell"/> assigned to this detail.
        /// </summary>
        /// <remarks>Should be populated when this detail is assigned to a grid object is on the grid.</remarks>
        public GridCell? AssignedGridCell { get; set; }
    }
}
