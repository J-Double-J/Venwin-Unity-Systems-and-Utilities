using System;
using UnityEngine;
using Venwin.Utilities;

#nullable enable

namespace Venwin.Grid
{
    public class Projected3DSquareGrid<GridCellT, T> : Grid<GridCellT, T> where GridCellT : GridCell<T> where T : GridObject
    {
        /// <summary>
        /// Gets the details for how projection should work for this grid.
        /// </summary>
        protected ProjectionDetails ProjectionDetails { get; set; }

        public Projected3DSquareGrid(ProjectionDetails projectionDetails,
            Transform transform,
            Mesh mesh,
            int cellSize,
            int yAxisMax,
            LayerMask gridLayer,
            bool isNavigatable,
            Func<Grid, int, Vector3Int, Vector3, GridCellT>? callback = null)
            : base(transform, mesh, cellSize, yAxisMax, gridLayer, isNavigatable, callback)
        {
            ProjectionDetails = projectionDetails;
        }

        #region Cell Creation

        /// <inheritdoc/>
        protected override void CreateGridCells()
        {
            for (int col = 0; col < ColumnCount; col++)
            {
                for (int row = 0; row < RowCount; row++)
                {
                    float halfCellSize = CellSize / 2.0f;
                    Vector3 centerOfCellRayStart = new(col * CellSize + BottomLeftCorner.x + halfCellSize,
                                                        ProjectionDetails.StartingYIndexAboveGrid + BottomLeftCorner.y,
                                                        row * CellSize + BottomLeftCorner.z + halfCellSize);

                    Vector3Int xzGridCoord = new(col, 0, row);

                    CastRayAndDetermineCells(centerOfCellRayStart, xzGridCoord);
                }
            }
        }

        /// <summary>
        /// Casts a ray downwards from a position and does math to determine automatic GridCell placement.
        /// </summary>
        /// <param name="centerOfCellRayStart">Some position over the center of a cell on the grid surface.</param>
        /// <param name="xzGridCoord">The XZ coordinate of the cell position being raycasted through</param>
        protected virtual void CastRayAndDetermineCells(Vector3 centerOfCellRayStart, Vector3Int xzGridCoord)
        {
            float halfCellSize = CellSize / 2.0f;

            // Use the first grid surface found as the only cell.
            if (!ProjectionDetails.CastThroughAllObjects)
            {
                Ray ray = new(centerOfCellRayStart, Vector3.down);
                if (Physics.Raycast(ray,
                    out RaycastHit hitInfo,
                    ProjectionDetails.StartingYIndexAboveGrid - ProjectionDetails.LowestYIndex,
                    ProjectionDetails.GridSurfaceLayer,
                    QueryTriggerInteraction.Ignore))
                {
                    Vector3Int pointInt = MathUtilities.VectorFloatToInt_WithErrorThreshold(hitInfo.point);

                    Vector3 worldSpaceCoordinates = new(pointInt.x,
                                                        pointInt.y,
                                                        pointInt.z);

                    Vector3Int gridCoord = new(xzGridCoord.x, pointInt.y, xzGridCoord.z);

                    GridCell cell;
                    cell = CreateGridCell(worldSpaceCoordinates, gridCoord);

                    if (hitInfo.normal != Vector3.up)
                    {
                        cell.CellRampDetails = GridCellRampDetails.CreateRampDetails(hitInfo, centerOfCellRayStart, CellSize, gridCoord);
                    }

                    GridCells[gridCoord] = cell;
                }

                return;
            }

            // Grab all the game objects at the given x-z position
            RaycastHit[] hits = Physics.RaycastAll(centerOfCellRayStart,
                                                    Vector3.down,
                                                    ProjectionDetails.StartingYIndexAboveGrid - ProjectionDetails.LowestYIndex,
                                                    ProjectionDetails.GridSurfaceLayer,
                                                    QueryTriggerInteraction.Ignore);

            // Descending order
            Array.Sort(hits, (a, b) => b.point.y.CompareTo(a.point.y));

            // Arbitrary value for "not set"
            int yLayerOfLastSurfaceFound = Mathf.FloorToInt(ProjectionDetails.StartingYIndexAboveGrid) + CellSize;

            foreach (RaycastHit hit in hits)
            {

                Vector3Int pointInt = MathUtilities.VectorFloatToInt_WithErrorThreshold(hit.point);

                // The top of our hit surface is the bottom of an occupied cell
                if (yLayerOfLastSurfaceFound - pointInt.y <= CellSize)
                {
                    // We take the top of the surface we found and record that as last found surface.
                    // We still record it in here because it's still blocking the cell its in.
                    yLayerOfLastSurfaceFound = (int)Math.Ceiling(Math.Round(hit.point.y, 2)); // This can happen: 2.0000000001 -> 2.00 -> 2
                    continue;
                }

                // We take the top of the surface we found and record that as last found surface.
                yLayerOfLastSurfaceFound = (int)Math.Ceiling(Math.Round(hit.point.y, 2)); // This can happen: 2.0000000001 -> 2.00 -> 2

                Vector3 worldSpaceCoordinates = new(pointInt.x,
                                                    pointInt.y,
                                                    pointInt.z);

                Vector3Int gridCoord = new(xzGridCoord.x, pointInt.y, xzGridCoord.z);

                GridCell gridCell = CreateGridCell(worldSpaceCoordinates, gridCoord);

                if (hit.normal != Vector3.up)
                {
                    gridCell.CellRampDetails = GridCellRampDetails.CreateRampDetails(hit, centerOfCellRayStart, CellSize, gridCoord);
                }

                GridCells[gridCoord] = gridCell;

            }
        }

        /// <summary>
        /// Creates the <see cref="GridCell"/> when a cell is determined to exist at a position.
        /// </summary>
        /// <remarks>
        /// If a class derives from <see cref="Projected3DSquareGrid"/> and just needs to change what cells are created, this is the method to change.
        /// </remarks>
        /// <param name="worldSpaceCoordinates">World Space Coordinates of the cell.</param>
        /// <param name="gridCoord">Grid Coordinates of the cell.</param>
        /// <returns>The created <see cref="GridCell"/>.</returns>
        protected virtual GridCell CreateGridCell(Vector3 worldSpaceCoordinates, Vector3Int gridCoord)
        {
            GridCell cell;
            if (CellCreationCallback == null)
            {
                cell = new GridCell<T>(this, CellSize, gridCoord, worldSpaceCoordinates);
            }
            else
            {
                cell = CellCreationCallback(this, CellSize, gridCoord, worldSpaceCoordinates);
            }

            return cell;
        }

        #endregion Cell Creation

        #region Cell Navigation

        /// <inheritdoc/>
        protected override void ConfigureGridCellsForNavigation()
        {
            for (int col = 0; col < ColumnCount; col++)
            {
                for (int yAxis = 0; yAxis < YAxisMax; yAxis++)
                {
                    for (int row = 0; row < RowCount; row++)
                    {
                        if (!GridCells.TryGetValue(new Vector3Int(col, yAxis, row), out GridCell cell))
                        {
                            continue;
                        }

                        if (cell.CellRampDetails == null)
                        {
                            AssignCellNeighborsForNonRamps(cell);
                        }
                        else
                        {
                            AssignCellNeighborsForRamps(cell);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Finds the <paramref name="currentCell"/>'s neighbors and makes it aware of them.
        /// </summary>
        /// <param name="currentCell">The cell getting neighbors</param>
        protected virtual void AssignCellNeighborsForNonRamps(GridCell currentCell)
        {
            // North
            CheckForNeighborEligibilityAtGridCoord(currentCell, GridDirection.North);

            // East
            CheckForNeighborEligibilityAtGridCoord(currentCell, GridDirection.East);

            // South
            CheckForNeighborEligibilityAtGridCoord(currentCell, GridDirection.South);

            // West
            CheckForNeighborEligibilityAtGridCoord(currentCell, GridDirection.West);
        }

        protected virtual void CheckForNeighborEligibilityAtGridCoord(GridCell currentCell, GridDirection gridDirection)
        {
            if (currentCell.CellRampDetails != null)
            {
                throw new InvalidOperationException($"Cannot do neighbor eligibility checks using ramp logics when the current cell doesn't have a ramp. " +
                    $"Use {nameof(CheckForNeighborEligibilityAtGridCoord_Ramp)} instead.");
            }

            Vector3Int gridCoord = currentCell.GridCoordinates + gridDirection.GetVectorFromDirection();
            if (IsValidCellCoordinate(gridCoord))
            {
                GridCell eligibleCell = GridCells[gridCoord];

                // If the eligible cell is a ramp that is not oriented in the direction we are approaching from, its not a neighbor.
                if (eligibleCell.CellRampDetails != null && eligibleCell.CellRampDetails.UpperRampDirection != gridDirection)
                {
                    return;
                }

                currentCell.AddNeighbor(eligibleCell);
            }
            else
            {
                CheckForLowerRampNeighbor(currentCell, gridCoord, gridDirection);
            }
        }

        /// <summary>
        /// Assigns neighbors for ramps.
        /// </summary>
        /// <remarks>
        /// Default implementation treat ramps as one directional cells that don't allow "turning" off the ramp until completely off it.
        /// </remarks>
        /// <example>
        /// R - Ramp
        /// O - Neighbor
        /// X - Non-neighbor
        /// 
        /// XXX      XOX
        /// ORO  OR  XRX
        /// XXX      XOX
        /// </example>
        /// <param name="currentCell">The cell being evaluated</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="currentCell"/> is not a ramp.</exception>
        protected virtual void AssignCellNeighborsForRamps(GridCell currentCell)
        {
            if (currentCell.CellRampDetails == null)
            {
                throw new ArgumentException($"Grid Cell must have populated {nameof(GridCellRampDetails)} in order to create neighbors with ramp logic.");
            }

            CheckForNeighborEligibilityAtGridCoord_Ramp(currentCell, currentCell.CellRampDetails.UpperRampDirection, true);
            CheckForNeighborEligibilityAtGridCoord_Ramp(currentCell, currentCell.CellRampDetails.DownRampDirection, false);
        }

        /// <summary>
        /// Checks if a neighbor is eligible for a grid cell.
        /// </summary>
        /// <remarks>
        /// This logic is executed for grid cells with ramps.
        /// </remarks>
        /// <param name="currentCell"></param>
        /// <param name="gridDirection">Direction to search in.</param>
        /// <param name="searchUpperDirection">If true, uses logic in the upperwards direction, else uses the downward direction</param>
        protected virtual void CheckForNeighborEligibilityAtGridCoord_Ramp(GridCell currentCell, GridDirection gridDirection, bool searchUpperDirection)
        {
            if (currentCell.CellRampDetails == null)
            {
                throw new InvalidOperationException("Cannot do neighbor eligibility checks using ramp logics when the current cell doesn't have a ramp. " +
                    $"Use {nameof(CheckForNeighborEligibilityAtGridCoord)} instead.");
            }

            Vector3Int gridCoord = searchUpperDirection ? currentCell.GridCoordinates + gridDirection.GetVectorFromDirection() + Vector3Int.up
                                                        : currentCell.GridCoordinates + gridDirection.GetVectorFromDirection();
            if (IsValidCellCoordinate(gridCoord))
            {
                GridCell eligibleCell = GridCells[gridCoord];

                // If the eligible cell is a ramp that is not oriented in the direction we are approaching from, its not a neighbor.
                if (eligibleCell.CellRampDetails != null && eligibleCell.CellRampDetails.UpperRampDirection != gridDirection)
                {
                    return;
                }

                currentCell.AddNeighbor(eligibleCell);
            }
            else
            {
                CheckForLowerRampNeighbor(currentCell, gridCoord, gridDirection);
            }
        }

        /// <summary>
        /// Checks if a grid ramp exists on a lower y-index and adds that as a neighbor.
        /// </summary>
        /// <param name="currentCell">The current cell having neighbors assigned to.</param>
        /// <param name="gridCellOffsetOfCurrent">A position that has been searched for a neighbor that needs to be searched below of.</param>
        /// <param name="gridDirection">Direction of approach, used to see if its going the same direction downwards.</param>
        protected virtual void CheckForLowerRampNeighbor(GridCell currentCell, Vector3Int gridCellOffsetOfCurrent, GridDirection gridDirection)
        {
            // If a ramp is sloping down into another ramp, we need to look further down rather than the current ramps lower point.
            // The current ramp's lower point is even with the floor, but other ramps are likely one step lower.
            Vector3Int downAndOffset = gridCellOffsetOfCurrent + new Vector3Int(0, -1, 0);

            if (IsValidCellCoordinate(downAndOffset))
            {
                if (GridCells[downAndOffset].CellRampDetails != null && GridCells[downAndOffset].CellRampDetails!.DownRampDirection == gridDirection)
                {
                    currentCell.AddNeighbor(GridCells[downAndOffset]);
                }
            }
        }

        #endregion Cell Navigation
    }
}
