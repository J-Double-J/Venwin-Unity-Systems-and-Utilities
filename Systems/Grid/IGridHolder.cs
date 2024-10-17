

namespace Venwin.Grid
{
    /// <summary>
    /// Designates a class as holding a grid.
    /// </summary>
    public interface IGridHolder
    {
        public Grid HeldGrid { get; }
    }

    /// <summary>
    /// Designates a class as holding a grid that takes objects of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Type that is held by grid, that derives from <see cref="GridObject"/>.</typeparam>
    public interface IGridHolder<T> : IGridHolder where T : GridObject
    {
        public new Grid<T> HeldGrid { get; }

        // Default implementation of the non-generic HeldGrid property
        Grid IGridHolder.HeldGrid => HeldGrid;
    }
}
