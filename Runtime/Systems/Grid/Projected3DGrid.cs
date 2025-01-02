using System;
using UnityEditor.PackageManager;
using UnityEngine;
using Venwin.Utilities;

#nullable enable

namespace Venwin.Grid
{
    /// <summary>
    /// Differs from the regular <see cref="Grid"/> in that it can detect the y-dimension automatically with the use of raycasting.
    /// </summary>
    public class Projected3DGrid : Grid
    {
        /// <summary>
        /// Gets the details for how projection should work for this grid.
        /// </summary>
        protected ProjectionDetails ProjectionDetails { get; set; }

        public Projected3DGrid(ProjectionDetails projectionDetails, Transform transform, Mesh mesh, int cellSize, int yAxisMax, LayerMask gridLayer, Func<Grid, int, Vector3Int, Vector3, GridCell>? callback)
            : base(transform, mesh, cellSize, yAxisMax, gridLayer, callback, false)
        {
            ProjectionDetails = projectionDetails;
        }

        /// <inheritdoc/>
        protected override void CreateGridCells()
        {
            for(int col = 0; col < ColumnCount; col++)
            {
                for(int row = 0; row < RowCount; row++)
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
                if(Physics.Raycast(ray,
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
                    if(CellCreationCallback == null)
                    {
                        cell = new GridCell(this, CellSize, gridCoord, worldSpaceCoordinates);
                    }
                    else
                    {
                        cell = CellCreationCallback(this, CellSize, gridCoord, worldSpaceCoordinates);
                    }

                    if(hitInfo.normal != Vector3.up)
                    {
                        cell.CellRampDetails = GridCellRampDetails.CreateRampDetails(hitInfo, centerOfCellRayStart, CellSize);
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

            foreach(RaycastHit hit in hits)
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

                GridCell gridCell;
                if (CellCreationCallback == null)
                {
                    gridCell = new GridCell(this, CellSize, gridCoord, worldSpaceCoordinates);
                }
                else
                {
                    gridCell = CellCreationCallback(this, CellSize, gridCoord, worldSpaceCoordinates);
                }

                if(hit.normal != Vector3.up)
                {
                    gridCell.CellRampDetails = GridCellRampDetails.CreateRampDetails(hit, centerOfCellRayStart, CellSize);
                }

                GridCells[gridCoord] = gridCell;

            }
        }

        /// <inheritdoc/>
        protected override void ConfigureGridCellsForNavigation()
        {
            throw new NotImplementedException("Ensure the other stuff is complete dummy!");
            for (int col = 0; col < ColumnCount; col++)
            {
                for(int yAxis = 0; yAxis < YAxisMax; yAxis++)
                {
                    for (int row = 0; row < RowCount; row++)
                    {
                        if(!GridCells.TryGetValue(new Vector3Int(col, yAxis, row), out GridCell cell))
                        {
                            continue;
                        }
                    }
                }
            }
        }
    }
}
