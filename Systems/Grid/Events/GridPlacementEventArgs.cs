using System;

namespace Venwin.Grid
{
    public class GridPlacementEventArgs : EventArgs
    {
        public GridObject GridObject { get; private set; }

        public GridPlacementEventArgs(GridObject gridObject)
        {
            GridObject = gridObject;
        }
    }
}
