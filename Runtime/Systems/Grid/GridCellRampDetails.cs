using System;

using UnityEngine;

namespace Venwin.Grid
{
    public class GridCellRampDetails
    {
        /// <summary>
        /// The direction in relation to the grid that the higher end of the ramp is.
        /// </summary>
        /// <remarks>
        /// So if the value is <see cref="GridDirection.East"/> then the upper end of the ramp is closer to (1,0,0) than (0,0,0). <br/>
        /// It then also means that to go up one must travel east.
        /// </remarks>
        public GridDirection UpperRampDirection { get; private set; }

        /// <summary>
        /// The direction in relation to the grid that the lower end of the ramp is.
        /// </summary>
        /// <remarks>
        /// So if the value is <see cref="GridDirection.West"/> then the lower end of the ramp is closer to (0,0,0) than (1,0,0). <br/>
        /// It then also means that to go down one must travel west.
        /// </remarks>
        public GridDirection DownRampDirection { get; private set; }

        /// <summary>
        /// The level that the upper ramp is moving towards
        /// </summary>
        public int YIndexUpperRamp { get; private set; }

        /// <summary>
        /// The leval that the lower ramp is moving towards.
        /// </summary>
        public int YIndexLowerRamp { get; private set; }

        private GridCellRampDetails() { }

        /// <summary>
        /// Creates ramp details depending on the norrmal of a raycast on a surface.
        /// </summary>
        /// <remarks>
        /// Some requirements:
        /// <list type="bullet">
        ///     <item>Must be sloped continously in one direction.</item>
        ///     <item>Collider cannot be stair stepped.</item>
        ///     <item>Slopes can only go up 1 'CellSize' up or down.</item>
        ///     <item>Ramp collider covers most of a cell. The collider must exist 75% through the cell in all directions.</item>
        /// </list>
        /// 
        /// If these requirements are not followed, strange behavior may be introduced or a failure to create the ramp.
        /// </remarks>
        /// <param name="hitInfoAtCellXZCenter"></param>
        /// <param name="rayCastOrigin"></param>
        /// <param name="cellSize"></param>
        /// <param name="gridCoord"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">Thrown if trying to create ramp data from a flat surface.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the expectations between the raycast normals and the elevation changes are broken.<br/>
        /// Won't be thrown if the requirements are followed.</exception>
        public static GridCellRampDetails CreateRampDetails(RaycastHit hitInfoAtCellXZCenter, Vector3 rayCastOrigin, int cellSize, Vector3Int gridCoord)
        {
            GridCellRampDetails gridCellRampDetails = new GridCellRampDetails();
            Vector3 norm = hitInfoAtCellXZCenter.normal;
            LayerMask layerToQuery = 1 << hitInfoAtCellXZCenter.transform.gameObject.layer;

            Vector3 xStepAngleCheck = Vector3.zero;
            Vector3 zStepAngleCheck = Vector3.zero;
            float step = cellSize / 4f;
            bool determinedAngle = false;

            gridCellRampDetails.YIndexUpperRamp = gridCoord.y + cellSize;
            gridCellRampDetails.YIndexLowerRamp = gridCoord.y;

            xStepAngleCheck = new(rayCastOrigin.x + step, Mathf.Floor(hitInfoAtCellXZCenter.point.y) + cellSize, rayCastOrigin.z);
            zStepAngleCheck = new(rayCastOrigin.x, Mathf.Floor(hitInfoAtCellXZCenter.point.y) + cellSize, rayCastOrigin.z + step);

            
            if(xStepAngleCheck == Vector3.zero && zStepAngleCheck == Vector3.zero)
            {
                throw new NotSupportedException("Unclear how to interpret ramp data for a flat surface.");
            }

            RaycastHit? raycastHit = CheckAngleAtDifferentOrigin(xStepAngleCheck, cellSize, layerToQuery, hitInfoAtCellXZCenter);
            
            if(raycastHit != null)
            {
                // We haven't had a difference in elevation
                if((float)Math.Round(raycastHit.Value.point.y, 1) > (float)Math.Round(hitInfoAtCellXZCenter.point.y, 1))
                {
                    determinedAngle = true;
                    gridCellRampDetails.SetHigherEnd(GridDirection.East);
                }
                else if((float)Math.Round(raycastHit.Value.point.y, 1) < (float)Math.Round(hitInfoAtCellXZCenter.point.y, 1))
                {
                    determinedAngle = true;
                    gridCellRampDetails.SetHigherEnd(GridDirection.West);
                }
            }

            if(determinedAngle)
            {
                return gridCellRampDetails;
            }

            // Check z-axis
            raycastHit = CheckAngleAtDifferentOrigin(zStepAngleCheck, cellSize, layerToQuery, hitInfoAtCellXZCenter);

            if (raycastHit != null)
            {
                // We haven't had a difference in elevation
                if ((float)Math.Round(raycastHit.Value.point.y, 1) > (float)Math.Round(hitInfoAtCellXZCenter.point.y, 1))
                {
                    determinedAngle = true;
                    gridCellRampDetails.SetHigherEnd(GridDirection.North);
                    return gridCellRampDetails;

                }
                else if ((float)Math.Round(raycastHit.Value.point.y, 1) < (float)Math.Round(hitInfoAtCellXZCenter.point.y, 1))
                {
                    determinedAngle = true;
                    gridCellRampDetails.SetHigherEnd(GridDirection.South);
                    return gridCellRampDetails;
                }
            }

            throw new InvalidOperationException("Something went wrong, a ramp was attempted to be discerned but was unable to determine details");
        }

        /// <summary>
        /// Tries to raycast hit the same object hit in the <paramref name="originalHit"/>.
        /// </summary>
        /// <remarks>
        /// This is a helper function to ensure that we can hit the object again as we deduce ramp details.
        /// </remarks>
        /// <param name="rayOrigin">The position of a new ray cast.</param>
        /// <param name="cellSize">The grid's cell size.</param>
        /// <param name="layerToQuery">Layer to hit.</param>
        /// <param name="originalHit">The RaycastHit info that originally triggered the ramp creation.</param>
        /// <returns>A raycast hit if the game object was hit again, else null.</returns>
        private static RaycastHit? CheckAngleAtDifferentOrigin(Vector3 rayOrigin, int cellSize, LayerMask layerToQuery, RaycastHit originalHit)
        {
            RaycastHit[] hits = Physics.RaycastAll(rayOrigin, Vector3.down, 2 * cellSize, layerToQuery, QueryTriggerInteraction.Ignore);

            RaycastHit? verifiedHit = null;
            foreach (var hit in hits)
            {
                // Verifying we hit the same object again
                if (hit.collider == originalHit.collider)
                {
                    verifiedHit = hit;
                    break;
                }
            }

            return verifiedHit;
        }

        /// <summary>
        /// Sets the end that is higher and then sets the opposite for the lower end.
        /// </summary>
        /// <param name="rampDirectionOnHigherSide">Direction that is on the higher end of the ramp.</param>
        private void SetHigherEnd(GridDirection rampDirectionOnHigherSide)
        {
            UpperRampDirection = rampDirectionOnHigherSide;
            
            switch(rampDirectionOnHigherSide)
            {
                case GridDirection.North:
                    DownRampDirection = GridDirection.South;
                    break;
                case GridDirection.East:
                    DownRampDirection = GridDirection.West;
                    break;
                case GridDirection.West:
                    DownRampDirection = GridDirection.East;
                    break;
                case GridDirection.South:
                    DownRampDirection = GridDirection.North;
                    break;
            }
        }
    }
}
